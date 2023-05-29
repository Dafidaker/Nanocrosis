using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct DoctorNotificationValues
{
    public GameObject doctorNotification;
    public GameObject title;
    public TextMeshProUGUI tileText;
    public GameObject subTitle;
    public TextMeshProUGUI subTitleText;
}

[Serializable]
public class Notification
{
    public string identification;
    public float cooldown;
   [HideInInspector] public bool avaliable;
    public float duration;
    [TextArea(1,3)] public string title;
    [TextArea(1,3)] public string subTitle;
}

public class DoctorManager : MonoBehaviour
{
    public static DoctorManager Instance;

    [SerializeField] private bool ShowNotifications;
    
    [HideInInspector] private Queue<Notification> _notificationsQueue;
    
    [SerializeField] private DoctorNotificationValues doctorNot;
    [SerializeField] private List<Notification> notifications;
    [SerializeField] private float timeBetweenNotifications;
    
    private bool _notificationAreBeingShown;
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _notificationsQueue = new Queue<Notification>();

        foreach (var notification in notifications)
        {
            notification.avaliable = true;
        }
        
        AddToQueue("Move1");
        AddToQueue("Move2");
        AddToQueue("Guns");
        AddToQueue("Special Ammo");
    }

    
    
    public void AddToQueue(String id)
    {
        if (!ShowNotifications) return;
        
        var notification = GetNotification(id);
        if (notification.duration == 0f || !notification.avaliable) return;
        
        notification.avaliable = false;
        //Debug.Log("added: " + notification.identification);
        
        _notificationsQueue.Enqueue(notification);
        
        if (_notificationAreBeingShown) return;

        StartCoroutine(ShowNotification());
    }

    private Notification GetNotification(String id)
    {
        foreach (var notification in notifications)
        {
            if (notification.identification == id)
            {
                return notification;
            }
        }

        return new Notification();
    }

    private IEnumerator ShowNotification()
    {
        AudioManager.Instance.PlaySFX("Notification");
        var notification = _notificationsQueue.Dequeue();
        _notificationAreBeingShown = true;
        if (notification.title.Length > 0)
        {
            doctorNot.title.SetActive(true);
            doctorNot.tileText.text = notification.title;
        }
        if (notification.subTitle.Length > 0)
        {
            doctorNot.subTitle.SetActive(true);
            doctorNot.subTitleText.text = notification.subTitle;
        }
        doctorNot.doctorNotification.SetActive(true);
        
        yield return new WaitForSeconds(notification.duration);
        
        doctorNot.doctorNotification.SetActive(false);
        doctorNot.title.SetActive(false);
        doctorNot.subTitle.SetActive(false);

        StartCoroutine(SetAsAvaliable(notification));
        
        yield return new WaitForSeconds(timeBetweenNotifications);
        
        StartCoroutine(SetAsAvaliable(notification));
        
        if (_notificationsQueue.Count > 0)
        {
            StartCoroutine(ShowNotification());
        }
        else
        {
            _notificationAreBeingShown = false;
        }
    }

    private void Update()
    {
        /*foreach (var t in notifications.Where(t => !(t.cooldown > 0)))
        {
            t.cooldown -= Time.deltaTime;
        }*/

        /*if (Input.GetKeyDown(KeyCode.A))
        {
            AddToQueue("LungsLostHealth");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            AddToQueue("LungsGainedHealth");
        }*/ 
    }


    private IEnumerator SetAsAvaliable(Notification notification)
    {
        yield return new WaitForSeconds(notification.cooldown);
        notification.avaliable = true;
    }
}
