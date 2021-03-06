﻿using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Networking;
using System.Collections;

public class Gun_PulseGun : Gun
{
    [SerializeField]
    float
        force;

    RawImage crosshair;

    protected override void Start()
    {
        base.Start();

        uiCharges = GameObject.Find("Pulse Indicator").GetComponent<Expand>();
        crosshair = GameObject.Find("Crosshair Icon").GetComponent<RawImage>();

        GetComponent<Player_Shoot>().EventPulse += Shoot;
        rayCastLayerMask = 1 << 10;
        primarySoundName = "pulsegun_primary";
        secondarySoundName = "pulsegun_secondary";
        //reloadTime = 5;
        range = 20;
        maxCharges = 1;
        maxChargeTime = 1;
        charges = maxCharges;
    }

    protected override void Update()
    {
        base.Update();

        if (!isLocalPlayer)
        {
            return;
        }

        ShowIndicator();
    }

    void ShowIndicator()
    {
        RaycastHit hit = ShootRayCast();

        bool hitSomething = false;
        Collider[] objectsHit = Physics.OverlapSphere(hit.point, Vector3.Distance(transform.position, hit.point));
        foreach (Collider c in objectsHit)
        {
            hitSomething |= (c.tag == "Player" && !c.gameObject.GetComponent<Player_Setup>().isLocalPlayer) || c.tag == "Ball";
        }

        crosshair.color = hitSomething ? Color.red : Color.white;
    }


    protected override void ShootPrimary(string objectHit, Vector3 point, float charge)
    {
        base.ShootPrimary(objectHit, point, charge);

        float f = -force;
        ShootPulseGun(objectHit, point, new Vector3(f, f, f));
    }

    protected override void ShootSecondary(string objectHit, Vector3 point, float charge)
    {
        base.ShootSecondary(objectHit, point, charge);

        float f = force / 2;
        ShootPulseGun(objectHit, point, new Vector3(f, -f, f));
    }

    void ShootPulseGun(string objectHit, Vector3 point, Vector3 dir)
    {
        // DEBUG
        // GameObject m = Instantiate(Resources.Load<GameObject>("Prefabs/Magnet") as GameObject, point, Quaternion.identity) as GameObject;

        Collider[] objectsHit = Physics.OverlapSphere(point, Vector3.Distance(transform.position, point));
        for (int n = 0; n < objectsHit.Length; n++)
        {
            GameObject obj = objectsHit [n].gameObject;
            if (obj.tag == "Player" || obj.tag == "PhysicsObject" || obj.tag == "Ball")
            {
                Pulse(obj, dir);
            }
        }

        StartCoroutine(ShootTimer(reloadTime));
    }

    void Pulse(GameObject obj, Vector3 dir)
    {
        Vector3 extraAngle = new Vector3(0, 2, 0); // Schiet objecten iets omhoog
        Vector3 direction = Vector3.Scale(Vector3.Normalize(transform.position - (obj.transform.position + extraAngle)), dir);

        if (obj.tag == "Player" && obj.name != gameObject.name)
        {
            if (isServer)
            {
                GM_Flag flag = obj.GetComponentInChildren<GM_Flag>();
                if (flag != null)
                {
                    flag.CmdChangeFlagHolder("");
                }
            }
            obj.gameObject.GetComponent<Player_Force>().AddImpact(direction, (2 * direction.magnitude) / obj.transform.localScale.x);
        } else if (obj.tag == "PhysicsObject" || obj.tag == "Ball")
        {
            obj.GetComponent<Rigidbody>().AddForce(direction * 25);
            if (obj.tag == "Ball")
            {
                obj.GetComponent<GM_Ball>().lastHitBy = transform.name;
            }
        }
    }

    public override RaycastHit ShootRayCast()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 point = ray.origin + (ray.direction * range);
        hit.point = point;

        return hit;
    }
}
