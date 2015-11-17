﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Gun : NetworkBehaviour
{
    protected AudioSource audioSource;
    protected string soundName;

    protected Camera cam;
    protected Image reloadBar;
    protected Vector3 startScale;
    protected Vector3 targetScale;
    protected LayerMask rayCastLayerMask;

    public bool canShoot;
    protected float reloadTime, maxChargeTime, charge;
    protected int range;

    protected virtual void Start()
    {        
        audioSource = GetComponent<AudioSource>();
        cam = GetComponentInChildren<Camera>();
        //GetComponent<Player_Shoot>().EventShoot += Shoot;
        rayCastLayerMask = ~((1 << 9) | (1 << 2));
        canShoot = true;
        range = 200;
        maxChargeTime = 3;
    }

    /*public void UnsubscribeEvent()
    {
        GetComponent<Player_Shoot>().EventShoot -= Shoot;
    }*/

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (!canShoot)
        {
            reloadBar.transform.localScale = Vector3.Lerp(reloadBar.transform.localScale, targetScale, 3 * (1 / reloadTime) * Time.deltaTime);
        } else
        {
            reloadBar.transform.localScale = targetScale;
        }
    }

    public void Shoot(string objectHit, Vector3 point, float charge, bool isPrimary)
    {
        if (isPrimary)
        {
            ShootPrimary(objectHit, point, charge);
        } else
        {
            ShootSecondary(objectHit, point, charge);
        }

        AudioClip audioClip = Resources.Load<AudioClip>("Sounds/snd_" + soundName);
        audioSource.PlayOneShot(audioClip);

        Discharge();
    }

    protected virtual void ShootPrimary(string objectHit, Vector3 point, float charge)
    {

    }
    
    protected virtual void ShootSecondary(string objectHit, Vector3 point, float charge)
    {
        
    }

    public void ChargeGun(float timeHeld)
    {
        charge = Mathf.Clamp(charge + timeHeld, 0, maxChargeTime);
    }

    void Discharge()
    {
        charge = 0;
    }

    public virtual RaycastHit ShootRayCast()
    {
        RaycastHit hit;
        Ray ray = new Ray(cam.transform.TransformPoint(0, 0, 0.5f), cam.transform.forward);

        if (Physics.Raycast(ray, out hit, range, rayCastLayerMask))
        { 
            //Debug.DrawRay(cam.transform.TransformPoint(0, 0, 0.5f), cam.transform.forward * hit.distance, Color.blue, 10);
            reloadBar.transform.localScale = startScale;

            return hit;
        }

        return new RaycastHit();
    }

    protected IEnumerator ShootTimer(float seconds)
    {
        canShoot = false;
        yield return new WaitForSeconds(seconds);
        canShoot = true;
    }

    public bool CanShoot
    {
        get { return canShoot; }
    }

    public float Charge
    {
        get { return charge; }
    }
}
