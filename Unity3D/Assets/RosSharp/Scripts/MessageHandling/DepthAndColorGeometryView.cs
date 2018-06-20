//using UnityEngine;
//using System.Collections;
//using System;

//public class DepthAndColorGeometryView : MonoBehaviour {

//    string depthTopic;
//    string colorTopic;
//    int framerate = 100;
//    public string compression = "none"; //"png" is the other option, haven't tried it yet though
//    string depthMessage;
//    string colorMessage;

//    public Material Material;
//    Texture2D depthTexture;
//    Texture2D colorTexture;

//    int width = 512;
//    int height = 424;

//    Matrix4x4 m;

//    // Use this for initialization
//    void Start() {
//        // Create a texture for the depth image and color image
//        depthTexture = new Texture2D(width, height, TextureFormat.R16, false);
//        colorTexture = new Texture2D(2, 2);
//    }

//    // Update is called once per frame
//    void UpdateTexture(Message depthMessage, Message colorMessage) {
//        try {
//            byte[] depthImage = (SensorImage)depthMessage.data;

//            depthTexture.LoadRawTextureData(depthImage);
//            //depthTexture.LoadImage(depthImage);
//            depthTexture.Apply();
//            //Debug.Log(depthTexture.GetType());

//        }
//        catch (Exception e) {
//            Debug.Log(e.ToString());
//        }

//        try {
//            colorMessage = wsc.messages[colorTopic];
//            byte[] colorImage = System.Convert.FromBase64String(colorMessage);
//            colorTexture.LoadImage(colorImage);
//            colorTexture.Apply();
//        }
//        catch (Exception e) {
//            Debug.Log(e.ToString());
//            return;
//        }
//    }

//    void OnRenderObject() {

//        Material.SetTexture("_MainTex", depthTexture);
//        Material.SetTexture("_ColorTex", colorTexture);
//        Material.SetPass(0);

//        m = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
//        Material.SetMatrix("transformationMatrix", m);

//        Graphics.DrawProcedural(MeshTopology.Points, 512 * 424, 1);
//    }
//}