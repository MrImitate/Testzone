﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Gun : NetworkBehaviour
{
    protected AudioSource audioSource;
    protected string soundName;

    public ParticleSystem primaryParticles, secondaryParticles;
    protected Camera cam;
    protected LayerMask rayCastLayerMask;

    public bool canShoot;
    protected float reloadTime, reloadTimeLeft, maxChargeTime, charge;
    protected int range, charges, maxCharges;

    protected expand uiCharges;

    // DEBUG
    public int timesShot;

    protected virtual void Start()
    {        
        audioSource = GetComponent<AudioSource>();
        cam = GetComponentInChildren<Camera>();
        uiCharges = GameObject.Find("Bar Overlap").GetComponent<expand>();
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
        ChargeCharges();

        if (!isLocalPlayer)
            return;

        /*if (!canShoot)
        {
            reloadBar.transform.localScale = Vector3.Lerp(reloadBar.transform.localScale, targetScale, 3 * (1 / reloadTime) * Time.deltaTime);
        } else
        {
            reloadBar.transform.localScale = targetScale;
        }*/
    }

    protected void ChargeCharges()
    {
        if (charges < maxCharges)
        {
            reloadTimeLeft -= Time.deltaTime;
            uiCharges.ChargeBar(charges, 1 - (reloadTimeLeft / reloadTime));
            if (reloadTimeLeft <= 0)
            {
                charges++;
                uiCharges.ChangeBarsVisible(charges);
                reloadTimeLeft = reloadTime;
            }
        } else
        {
            reloadTimeLeft = reloadTime;
        }
    }

    public void Shoot(string objectHit, Vector3 point, float charge, bool isPrimary)
    {
        if (charges > 0)
        {
            // DEBUG
            timesShot++;

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
    }

    protected virtual void ShootPrimary(string objectHit, Vector3 point, float charge)
    {
        charges -= (int)Mathf.Floor(Mathf.Clamp(charge, 1, 3));
        uiCharges.ChangeBarsVisible(charges);
    }
    
    protected virtual void ShootSecondary(string objectHit, Vector3 point, float charge)
    {
        charges -= (int)Mathf.Floor(Mathf.Clamp(charge, 1, 3));
        uiCharges.ChangeBarsVisible(charges);
    }

    public void ChargeGun(float timeHeld)
    {
        maxChargeTime = Mathf.Clamp(maxCharges, 0, charges);
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
            //reloadBar.transform.localScale = startScale;

            return hit;
        }

        return new RaycastHit();
    }

    protected IEnumerator ShootTimer(float seconds)
    {
        //canShoot = false;
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
    public int Charges
    {
        get { return charges; }
    }

}
