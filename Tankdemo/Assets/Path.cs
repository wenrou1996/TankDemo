using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path {

    //所有路点
    public Vector3[] waypoints;

    //当前路点索引
    public int index = -1;

    //当前的路点
    public Vector3 waypoint;

    //是否循环
    bool isLoop = false;

    //是否误差
    public float deviation = 5;

    //是否完成
    public bool isFinish = false;

    //是否到达目的地
    public bool IsReach(Transform trans)
    {
        Vector3 pos = trans.position;
        float distance = Vector3.Distance(waypoint, pos);
        return distance < deviation;
    }

    //下一个路点
    public void NextWaypoint()
    {
        //Debug.Log("123123123");
        if(index < 0)
        {
            return;
        }
        if(index < waypoints.Length - 1)
        {
            index++;
        }
        else
        {
            if(!isLoop)
            {
                index = 0;
            }
            else
            {
                //Debug.Log("12345");
                isFinish = true;
            }
        }
        waypoint = waypoints[index];
    }

    //根据场景标识物生成路点
    public void InitByObj(GameObject obj, bool isLoop = false)
    {
        int length = obj.transform.childCount;
        //没有子物体
        if(length == 0)
        {
            waypoints = null;
            index = -1;
            Debug.LogWarning("没有路点");
            return;
        }
        //遍历子物体生成路点
        waypoints = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            Transform trans = obj.transform.GetChild(i);
            waypoints[i] = trans.position;
        }
        //设置一些参数
        index = 0;
        waypoint = waypoints[index];
        this.isLoop = isLoop;
        isFinish = false;
    }

    //根据导航图初始化路径
    public void InitByNavMeshPath(Vector3 pos, Vector3 targetPos)
    {
        //Debug.Log("333333");
        //重置
        waypoints = null;
        index = -1;
        //计算路径
        NavMeshPath navPath = new NavMeshPath();
        bool hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);
        //Debug.Log("4444444");
        if(!hasFoundPath)
        {
            //Debug.Log("6666");
            return;
        }
        //Debug.Log("55555");
        //生成路径
        int length = navPath.corners.Length;
        waypoints = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            waypoints[i] = navPath.corners[i];
        }
        index = 0;
        waypoint = waypoints[index];
        isFinish = false;
        this.isLoop = true;
        //Debug.Log("111111");
    }

    //调试路径
    public void DrawWaypoints()
    {
        if(waypoints == null)
        {
            return;
        }
        int length = waypoints.Length;
        for (int i = 0; i < length; i++)
        {
            if(i == index)
            {
                Gizmos.DrawSphere(waypoints[i], 1);
            }
            else
            {
                Gizmos.DrawCube(waypoints[i], Vector3.one);
            }
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
