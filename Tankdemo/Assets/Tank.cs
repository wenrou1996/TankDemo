using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {

    //操控类型
    public enum CtrlType
    {
        none,
        player,
        computer
    }

    //默认操控类型为玩家
    public CtrlType ctrlType = CtrlType.player;

    //中心准心
    public Texture2D centerSight;

    //坦克准心
    public Texture2D tankSight;

    //生命指示条素材
    public Texture2D hpBarBg;
    public Texture2D hpBar;

    //焚烧特效
    public GameObject destoryEffect;

    //最大生命值
    private float maxHp = 100;

    //当前生命值
    public float hp = 100;

    //炮弹预设
    public GameObject bullet;

    //上一次开炮的时间
    public float lastShootTime = 0;

    //开炮的时间间隔
    private float shootInterval = 0.5f;

    //马达音源
    public AudioSource motorAudioSource;

    //马达音效
    public AudioClip motorClip;
	
    //履带
	private Transform tracks;

	//轮子
	private Transform wheels;

    //轮轴
    public List<Axlelnfo> axlelnfos;

    //马力/最大马力
    private float motor = 0;
    public float maxMotorTorque;

    //制动/最大制动
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;

    //转向角/最大转向角
    private float steering = 0;
    public float maxSteeringAngle;

    //炮塔
    public Transform turret;

    //炮塔旋转速度
    private float turretRotSpeed = 0.5f;

    //炮塔炮管目标角度
    private float turretRotTarget = 0;
    private float turretRollTarget = 0;

    //炮管
    public Transform gun;
    //炮管的旋转范围
    private float maxRoll = 10f;
    private float minRoll = -4f;


    //玩家控制
    public void PlayerCtrl()
    {
        //只有当玩家操控的坦克才会生效
        if(ctrlType != CtrlType.player)
        {
            return;
        }
        //马力和转向角
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
		//制动
		brakeTorque = 0;
		foreach (Axlelnfo axlenlnfo in axlelnfos) 
		{
			if (axlenlnfo.leftWheel.rpm > 5 && motor < 0) 
			{
				brakeTorque = maxBrakeTorque;
			} 
			else if (axlenlnfo.leftWheel.rpm < -5 && motor > 0) 
			{
				brakeTorque = maxBrakeTorque;
			}
			continue;
		}
        //炮塔炮管角度
        //turretRotTarget = Camera.main.transform.eulerAngles.y;
        //turretRollTarget = Camera.main.transform.eulerAngles.x;
        TargetSignPos();
        CalExplodePoint();
        //发射炮弹
        if(Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    //绘制生命条
    public void DrawHp()
    {
        //底框
        Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15, hpBarBg.width, hpBarBg.height);
        GUI.DrawTexture(bgRect, hpBarBg);
        //指示条
        float width = hp * 102 / maxHp;
        Rect hpRect = new Rect(bgRect.x + 27, bgRect.y + 6, width, hpBar.height);
        GUI.DrawTexture(hpRect, hpBar);
        //文字
        string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();
        Rect textRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);
        GUI.Label(textRect, text);
    }

    //绘制准心
    public void DrawSight()
    {
        //计算实际射击位置
        Vector3 explodePoint = CalExplodePoint();
        //获取坦克准心的屏幕坐标
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);
        //绘制坦克准心
        Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2, Screen.height - screenPoint.y - tankSight.height / 2, tankSight.width, tankSight.height);
        GUI.DrawTexture(tankRect, tankSight);
        //绘制中心准心
        Rect centerRect = new Rect(Screen.width/2 - centerSight.width/2, Screen.height/2 - centerSight.height/2, centerSight.width, centerSight.height);
        GUI.DrawTexture(centerRect, centerSight);
    }

    //计算角度
    public void TargetSignPos()
    {
        //碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit raycastHit;
        //屏幕中心位置
        Vector3 centerVec = new Vector3(Screen.width/2, Screen.height/2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);
        //射线检测，获取hitPiont
        if(Physics.Raycast(ray, out raycastHit, 400.0f))
        {
            hitPoint = raycastHit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        //计算目标角度
        Vector3 dir = hitPoint - turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;
        //调试用，稍后删除
        //Transform targetCube = GameObject.Find("TargetCube").transform;
        //targetCube.position = hitPoint;
    }

    //计算爆炸位置
    public Vector3 CalExplodePoint()
    {
        //碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        //沿着炮管方向的射线
        Vector3 pos = gun.position + gun.forward * 5;
        Ray ray = new Ray(pos, gun.forward);
        //射线检测
        if(Physics.Raycast(ray, out hit, 400.0f))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        //调试用，稍后将删除
        //Transform explodeCube = GameObject.Find("ExplodeCube").transform;
        //explodeCube.position = hitPoint;
        //调试用结束
        return hitPoint;
    }

    //开炮
    public void Shoot()
    {
        //发射间隔
        if(Time.time - lastShootTime < shootInterval)
        {
            return;
        }
        //子弹
        if(bullet == null)
        {
            return;
        }
        //发射
        Vector3 pos = gun.position + gun.forward * 50;
        Instantiate(bullet, pos, gun.rotation);
        lastShootTime = Time.time;
        BeAttacked(30);
    }

    //被击中
    public void BeAttacked(float att)
    {
        //坦克已经被摧毁
        if(hp <= 0)
        {
            return;
        }
        //击中处理
        if(hp > 0)
        {
            hp -= att;
        }
        //被摧毁
        if(hp <= 0)
        {
            GameObject destoryObj = (GameObject)Instantiate(destoryEffect);
            destoryObj.transform.SetParent(transform, false);
            destoryObj.transform.localPosition = Vector3.zero;
            ctrlType = CtrlType.none;
        }
    }

    //马达音效
    void MotorSound()
    {
        if (motor != 0 && !motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorClip;
            motorAudioSource.Play();
        }
        else if(motor == 0)
        {
            motorAudioSource.Pause();
        }
    }

	//履带滚动
	public void TrackMove()
	{
		if (tracks == null)
			return;
		float offset = 0;
		if (wheels.GetChild (0) != null)
			offset = wheels.GetChild (0).localEulerAngles.x / 90f;
		foreach (Transform track in tracks) 
		{
			MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer> ();
			if (mr == null)
				continue;
			Material mtl = mr.material;
			mtl.mainTextureOffset = new Vector2 (0, offset);
		}
	}

	//轮子旋转
	public void WheelsRotation(WheelCollider collider)
	{
		if (wheels == null)
			return;
		//获取旋转信息
		Vector3 position;
		Quaternion rotation;
		collider.GetWorldPose (out position, out rotation);
		//旋转每个轮子
		foreach (Transform wheel in wheels) 
		{
			wheel.rotation = rotation;
		}
	}

    //炮塔旋转
    public void TurretRotation()
    {
        if(Camera.main == null)
        {
            return;
        }
        if(turret == null)
        {
            return;
        }
        //归一化角度
        float angle = turret.eulerAngles.y - turretRotTarget;
        if(angle < 0)
        {
            angle += 360;
        }
        if(angle > turretRotSpeed && angle < 180)
        {
            turret.Rotate(0f, -turretRotSpeed, 0f);
        }
        else if(angle > 180 && angle < 360 - turretRotSpeed)
        {
            turret.Rotate(0f, turretRotSpeed, 0f);
        }
    }

    //炮管旋转
    public void TurretRoll()
    {
        if(Camera.main == null)
        {
            return;
        }
        if(turret == null)
        {
            return;
        }
        //获取角度
        Vector3 worldEuler = gun.eulerAngles;//相对于世界坐标系的角度
        Vector3 localEuler = gun.localEulerAngles;//相对于父坐标系的角度
        //世界坐标系角度计算
        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;
        //本地坐标系角度限制
        Vector3 euler = gun.localEulerAngles;
        if(euler.x > 180)//角度归一化，将角度范围从0-360变为-180-180
        {
            euler.x -= 360;
        }

        if(euler.x > maxRoll)//根据本地坐标系的角度，限制炮管的旋转范围
        {
            euler.x = maxRoll;
        }
        if(euler.x < minRoll)
        {
            euler.x = minRoll;
        }
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);//重新设置角度，炮管只在x轴旋转
        //System.Console.WriteLine("1111");
    }

	// Use this for initialization
	void Start () {
        //获取炮塔
        turret = transform.Find("turretMesh");
        //获取炮管
        gun = turret.Find("gunMesh");
		//获取轮子
		wheels = transform.Find("wheels");
		//获取履带
		tracks = transform.Find("chain");
        //马达音源
        motorAudioSource = gameObject.AddComponent<AudioSource>();
        motorAudioSource.spatialBlend = 1;
	}
	
	// Update is called once per frame
	void Update () {
        //玩家操作
        PlayerCtrl();
        //遍历车轴
        foreach (Axlelnfo axleInfo in axlelnfos)
        {
            //转向
            if(axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;

                axleInfo.rightWheel.steerAngle = steering;
            }
            //马力
            if(axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            //制动
            if(true)
            {
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
            }
			//转动轮子履带
//			if (axlelnfos [1] != null && axleInfo == axlelnfos[1]) 
//			{
//				WheelsRotation (axlelnfos [1].leftWheel);
//				TrackMove ();
//			}
        }
        //马达音效
        MotorSound();
        //炮塔旋转
        TurretRotation();
        //炮管旋转
        TurretRoll();
	}

	//绘图
	void OnGUI()
	{
        if (ctrlType != CtrlType.player)
            return;
        DrawSight();
        DrawHp();
	}
}
