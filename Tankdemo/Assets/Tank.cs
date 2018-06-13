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

    //最大生命值
    private float maxHp = 100;

    //当前生命值
    private float hp = 100;

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
        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
        //发射炮弹
        if(Input.GetMouseButton(0))
        {
            Shoot();
        }
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
}
