using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour {

    //单例
    public static Battle instance;

    //战场中的所有坦克
    public BattleTank[] battleTanks;

	// Use this for initialization
	void Start () {
        //单例
        instance = this;

        //开始战斗
        StartTwoCampBattle(2, 1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //获取阵营，0表示错误
    public int GetCamp(GameObject tankObj)
    {
        for (int i = 0; i < battleTanks.Length; i++)
        {
            BattleTank battleTank = battleTanks[i];
            if(battleTanks == null)
            {
                return 0;
            }
            if(battleTank.tank.gameObject == tankObj)
            {
                return battleTank.camp;
            }
        }
        return 0;
    }

    //是否属于同一阵营
    public bool IsSameCamp(GameObject tank1, GameObject tank2)
    {
        return GetCamp(tank1) == GetCamp(tank2);
    }

    //胜负判断 
    public bool IsWin(int camp)
    {
        for (int i = 0; i < battleTanks.Length; i++)
        {
            Tank tank = battleTanks[i].tank;
            if(battleTanks[i].camp != camp)
            {
                if(tank.hp > 0)
                {
                    return false;
                }
            }
        }
        Debug.Log("阵营" + camp + "获胜");
        return true;
    }

    //胜负判断
    public bool IsWin(GameObject attTank)
    {
        int camp = GetCamp(attTank);
        return IsWin(camp);
    }

    //清理场景
    public void ClearBattle()
    {
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < tanks.Length; i++)
        {
            Destroy(tanks[i]);
        }
    }
}
