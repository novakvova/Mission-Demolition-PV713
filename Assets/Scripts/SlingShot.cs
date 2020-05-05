using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class SlingShot : MonoBehaviour
{
    static private SlingShot S;

    [Header("Set in Inspector")]
    public GameObject prefabProjectile;
    public Material[] materials;
    public float velocityMult = 8f;
    [Header("Set Dynamically")]

    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile; // b
    public bool aimingMode;
    private Rigidbody projectileRigidbody;
    static public Vector3 LAUNCH_POS
    {
        get
        {
            if (S == null) return Vector3.zero;
            return S.launchPos;
        }
    }
    //*********************************************
    public Material Material_In;
    private int i = 0;
    //*********************************************



    void Start()
    {
        Invoke("GetRequest", 2f);
    }

    void GetRequest()
    {
        // PositionCollider positionCollider = Network.GetData().Result;
        var pc = Network.GetData().Result;

        projectile = Instantiate(prefabProjectile) as GameObject;

        if (i >= materials.Length)
        {
            i = 0;
        }
        Material[] mats = projectile.GetComponent<Renderer>().materials;
        mats[0] = materials[i];
        projectile.GetComponent<Renderer>().materials = mats;
        i++;

        // Сделать его кинематическим
        projectile.GetComponent<Rigidbody>().isKinematic = true;
        projectileRigidbody = projectile.GetComponent<Rigidbody>();
        projectileRigidbody.isKinematic = true;


        //Vector3 myPos = new Vector3(pc.pos); //positionCollider.pos;//
        projectile.transform.position = pc.pos;

        projectileRigidbody.isKinematic = false;

        //Vector3 v = new Vector3(14.7f, 17.5f, 0.0f);//positionCollider.velocity;
        projectileRigidbody.velocity = pc.velocity;

        FollowCam.POI = projectile;
        projectile = null;


        MissionDemolition.ShotFired(); // a
        ProjectileLine.S.poi = projectile;

        Invoke("GetRequest", 1f);
    }

    void Awake()
    {
        S = this;
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false); // b
        launchPos = launchPointTrans.position;
    }
    // Start is called before the first frame update
    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }
    void OnMouseExit()
    {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false); //    
    }
    void OnMouseDown()
    { // d
      // Игрок нажал кнопку мыши, когда указатель находился над рогаткой
        aimingMode = true;
        // Создать снаряд
        projectile = Instantiate(prefabProjectile) as GameObject;
        //List<Component> hingeJoints = new List<Component>();
        //projectile.GetComponents(typeof(GameObject), hingeJoints);
        //Debug.Log(hingeJoints.ToString());

        //******************************************************************************

        if (i >= materials.Length)
        {
            i = 0;
        }
        Material[] mats = projectile.GetComponent<Renderer>().materials;
        mats[0] = materials[i];
        projectile.GetComponent<Renderer>().materials = mats;
        i++;

        //******************************************************************************

        // Поместить в точку launchPoint
        projectile.transform.position = launchPos;
        // Сделать его кинематическим
        projectile.GetComponent<Rigidbody>().isKinematic = true;
        projectileRigidbody = projectile.GetComponent<Rigidbody>();
        projectileRigidbody.isKinematic = true;
    }
   

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{

        //}


        if (!aimingMode) return;
        Vector3 mousePos2D = Input.mousePosition; // с
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);
        Vector3 mouseDelta = mousePos3D - launchPos;
        // Ограничить mouseDelta радиусом коллайдера объекта Slingshot // d
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;
        if (Input.GetMouseButtonUp(0))
        { // e
          // Кнопка мыши отпущена
            aimingMode = false;
            projectileRigidbody.isKinematic = false;
            projectileRigidbody.velocity = -mouseDelta * velocityMult;

            FollowCam.POI = projectile;
            projectile = null;


            MissionDemolition.ShotFired(); // a
            ProjectileLine.S.poi = projectile;
        }
    }


}


public  class Network
{
    public static async Task<PositionCollider> GetData()
    {
        const string url = "http://52.155.182.96/api/game";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        var webResponse = request.GetResponse();
        var webStream = webResponse.GetResponseStream();
        var responseReader = new StreamReader(webStream);
        string response = responseReader.ReadToEnd();
        PositionCollider pc = JsonConvert.DeserializeObject<PositionCollider>(response);
        responseReader.Close();
        return pc;

    }

}

public class PositionCollider
{
  public Vector3 pos { get; set; }
  public Vector3 velocity { get; set; }

}