
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;

public class PlayerPositionSaver : UdonSharpBehaviour
{
    [Header("Interval in seconds between save ticks for each player.", order = 1)]
    public float TimeBetweenSaves = 60.0f;
    [Header("How many minutes the position gets remembered from the last time they left until they spawn at spawn again (0 = infinite).", order = 2)]
    public int RememberTime = 120;

    private float _elapsedTime = 0.0f;
    private VRCPlayerApi MyPlayer; //More efficient then getting the LocalPlayer each time

    void Start()
    {   //More efficient then getting the LocalPlayer each time
        MyPlayer = Networking.LocalPlayer;
    }

    private void Update()
{
        _elapsedTime += Time.deltaTime;
        //Has the given interval time been passed?
        if (_elapsedTime >= TimeBetweenSaves)
        {
            UpdatePlayerData();
            _elapsedTime = 0.0f;
        }

    }

    private void UpdatePlayerData()
    {
        //Runs for each player them self
        if (MyPlayer != null && MyPlayer.IsValid())
        {
            PlayerData.SetQuaternion("Rotation", MyPlayer.GetRotation());
            PlayerData.SetVector3("Position", MyPlayer.GetPosition());
            PlayerData.SetLong("LastTime", DateTime.Now.Ticks);
            Debug.Log("Player Position Updated.");
        }
    }
    
    public override void OnPlayerRestored(VRCPlayerApi Player)
    { //the funny new vrc function that makes this possible
        if (Player.isLocal) //only teleport ourself 
        { // we were here before?
            if (PlayerData.HasKey(Player, "LastTime"))
            { //does the time matter? 
                TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - PlayerData.GetLong(Player, "LastTime"));
                // total is all of them minutes only goes to 59
                Debug.Log("Elapsed time = " + elapsedSpan.TotalMinutes.ToString() + " max Time: " + RememberTime.ToString());
                if (elapsedSpan.TotalMinutes < RememberTime || RememberTime == 0)
                {  //put them where they were
                    Player.TeleportTo(PlayerData.GetVector3(Player, "Position"), PlayerData.GetQuaternion(Player, "Rotation"));
                }
            }
        }
    }
}