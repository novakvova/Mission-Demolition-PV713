using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

public class SlingShot : MonoBehaviour
{

    static private SlingShot S;
    public static string firstName;
    public static string secondName;
    public static bool isFirstShot;

    [Header("Set in Inspector")]
    public GameObject prefabProjectile;
    public Material[] materials;
    public float velocityMult = 8f;
    public float frequency = 0.5f;
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
        Invoke("GetRequest", frequency);
    }

    void GetRequest()
    {
        // PositionCollider positionCollider = Network.GetData().Result;
        var pc = Network.GetData(secondName).Result;
        if (pc == null)
        {
            Invoke("GetRequest", frequency);
        }
        else
        {
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

            //????????????
            Vector3 myPos = new Vector3(pc.pos.X, pc.pos.Y, pc.pos.Z); //positionCollider.pos;//
            projectile.transform.position = myPos;

            projectileRigidbody.isKinematic = false;

            //????????????
            Vector3 v = new Vector3(pc.velocity.X, pc.velocity.Y, pc.velocity.Z);//positionCollider.velocity;
            projectileRigidbody.velocity = v;

            FollowCam.POI = projectile;
            projectile = null;


            MissionDemolition.ShotFired(); // a
            ProjectileLine.S.poi = projectile;
            isFirstShot = true;
            Invoke("GetRequest", frequency);
        }
        
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
        if (isFirstShot)
        {
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
            isFirstShot = false;
            Network.PostData(firstName,projPos, projectileRigidbody.velocity);
            FollowCam.POI = projectile;
            projectile = null;

            MissionDemolition.ShotFired(); // a
            ProjectileLine.S.poi = projectile;

        }
    }


}


public  class Network
{
    public static async Task<PositionCollider> GetData(string nick)
    {
       
         string url = "http://52.171.228.182/api/game/" + nick;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        var webResponse = request.GetResponse();
        var webStream = webResponse.GetResponseStream();
        var responseReader = new StreamReader(webStream);
        string response = responseReader.ReadToEnd();
        Debug.Log("------------"+response);
        PositionCollider pc = JsonConvert.DeserializeObject<PositionCollider>(response);
        responseReader.Close();
        return pc;
        
    }

    public static void PostData(string nick,Vector3 pos, Vector3 velocity)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://52.171.228.182/api/game");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            PositionCollider pc = new PositionCollider
            {
                Nick = nick,
                pos = new PosVextor3 { X = pos.x, Y = pos.y, Z = pos.z },
                velocity = new PosVextor3 { X = velocity.x, Y = velocity.y, Z = velocity.z }
            };
            string json = JsonConvert.SerializeObject(pc);
            streamWriter.Write(json);
        }
        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }
}

public class PositionCollider
{
    public string Nick { get; set; }
    public PosVextor3 pos { get; set; }
    public PosVextor3 velocity { get; set; }

}

public class PosVextor3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}