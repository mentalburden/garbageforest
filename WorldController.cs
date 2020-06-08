using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.UI;

public class WorldController : MonoBehaviour
{
    Camera cam;
    public GameObject grass;
    public Color[] pix;
    public Texture2D dungeonmap;
    public int dungeonsize = 32;
    public float perlinscale = 0.2f;
    public GameObject perimwall;
    public GameObject[] treepossible;
    public GameObject[] bushpossible;
    public GameObject[] grasspossible;
    public GameObject spawner;
    public List<GameObject> spawners;
    private RawImage uimap;

    public static int getrand(int min, int max)
    {
        if (min >= max)
        {
            return min;
        }
        byte[] intBytes = new byte[4];
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetNonZeroBytes(intBytes);
        }
        return min + Math.Abs(BitConverter.ToInt32(intBytes, 0)) % (max - min + 1);
    }

    void calcmap()
    {
        int spawncounter = 0;
        pix = new Color[dungeonmap.width * dungeonmap.height];
        float seed = getrand(111, 999);
        float y = 0.0F;
        while (y < dungeonmap.height)
        {
            float x = 0.0F;
            while (x < dungeonmap.width)
            {
                float xCoord = dungeonsize + x / dungeonmap.width * perlinscale;
                float yCoord = dungeonsize + y / dungeonmap.height * perlinscale;
                float sample = Mathf.PerlinNoise(xCoord + seed, yCoord + seed);
                //item spawn pixels here
                if (sample <= 0.4 && sample >= 0.2)
                {
                    pix[(int)y * dungeonmap.width + (int)x] = Color.black;
                }
                else if (sample <= 0.43 && sample >= 0.41)
                {
                    pix[(int)y * dungeonmap.width + (int)x] = Color.red;
                }
                else if (sample <= 0.8 && sample >= 0.7)
                {
                    pix[(int)y * dungeonmap.width + (int)x] = Color.blue;

                }
                else if (sample >= 0.95)
                {
                    pix[(int)y * dungeonmap.width + (int)x] = Color.white;
                    spawncounter++;
                }
                x++;
            }
            y++;
        }
        if (spawncounter <= 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        dungeonmap.SetPixels(pix);
        dungeonmap.Apply();
        uimap.texture = dungeonmap;

    }

    void buildperimiter()
    {
        for (int x = 0; x < dungeonsize; x++)
        {
            Vector3 thiswallx = new Vector3(x, 0, 0);
            Vector3 thiswallz = new Vector3(0, 0, x);
            Vector3 thiswallxtop = new Vector3(dungeonsize, 0, x);
            Vector3 thiswallztop = new Vector3(x, 0, dungeonsize);
            Instantiate(perimwall, thiswallx, Quaternion.identity);
            Instantiate(perimwall, thiswallz, Quaternion.identity);
            Instantiate(perimwall, thiswallxtop, Quaternion.identity);
            Instantiate(perimwall, thiswallztop, Quaternion.identity);
        }
    }

    void buildforest()
    {
        //int spawnrand = getrand(1, dungeonsize);
        for (int x = 0; x < dungeonmap.width; x++)
        {
            for (int z = 0; z < dungeonmap.height; z++)
            {
                Color pixel = dungeonmap.GetPixel(x, z);
                if (pixel == Color.black)
                {
                    int arrayrand = getrand(0, treepossible.Length-1);
                    float scalerand = (getrand(1, 10) * 1.0f) / 10;
                    GameObject wallie = Instantiate(treepossible[arrayrand], new Vector3(x, scalerand, z), Quaternion.identity);
                    wallie.transform.localScale = new Vector3(0.5f, scalerand, 0.5f);
                    wallie.transform.position = new Vector3(wallie.transform.position.x, wallie.transform.position.y - 1.0f - scalerand, wallie.transform.position.z);
                    wallie.tag = "trees";
                }
                else if (pixel == Color.blue)
                {
                    int arrayrand = getrand(0, grasspossible.Length -1);
                    GameObject grassie = Instantiate(grasspossible[arrayrand], new Vector3(x, -1, z), Quaternion.identity);
                    grassie.tag = "grasses";

                }
                else if (pixel == Color.red)
                {
                    int arrayrand = getrand(0, bushpossible.Length -1);
                    GameObject bushie = Instantiate(bushpossible[arrayrand], new Vector3(x, -1, z), Quaternion.identity);
                    bushie.tag = "bushes";

                }
                else if (pixel == Color.white)
                {
                    GameObject spawnie = Instantiate(spawner, new Vector3(x, -1, z), Quaternion.identity);
                    spawnie.tag = "spawners";
                    spawners.Add(spawnie);

                }
            }
        }        
    }

    void movecam()
    {
        Vector3 spawnpos = spawners[getrand(1, spawners.Count)].transform.position;
        spawnpos.y += 1;
        Camera.main.transform.position = spawnpos;
    }

    void disablefade()
    {
        //change ui fade alpha later on
    }

    void Start()
    {
        try
        {
            grass.transform.localScale = new Vector3(dungeonsize * 2, 1, dungeonsize * 2);
            dungeonmap = new Texture2D(dungeonsize, dungeonsize);
            spawners = new List<GameObject>(dungeonsize * dungeonsize);
            uimap = GameObject.FindGameObjectWithTag("uimap").GetComponent<RawImage>(); ;
            calcmap();
            buildperimiter();
            buildforest();
            movecam();
            disablefade();
        }
        catch
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }



    void Update()
    {
         
    }
}
