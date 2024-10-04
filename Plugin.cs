using BepInEx;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace DougTheBugCamera
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject[] cameras = new GameObject[8];
        private Camera[] cameraProps = new Camera[8];
        private RenderTexture[] renderTextures = new RenderTexture[8];
        private GameObject displayCanvas;
        private GameObject Doug;
        private GameObject tintObject;
        private Image tintImage;

        void Start()
        {
            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }

        void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            for (int i = 0; i < 8; i++)
            {
                cameras[i] = new GameObject($"Camera_{i}");
                cameraProps[i] = cameras[i].AddComponent<Camera>();
                renderTextures[i] = new RenderTexture(Screen.width / 4, Screen.height / 4, 16);
                cameraProps[i].targetTexture = renderTextures[i];
                cameraProps[i].fieldOfView = mainCamera.fieldOfView;
                cameraProps[i].farClipPlane = 10000f;
                cameraProps[i].nearClipPlane = 0.25f;
                cameras[i].transform.position = mainCamera.transform.position;
                cameras[i].transform.rotation = mainCamera.transform.rotation;
                cameras[i].transform.position += new Vector3((i % 4) * 2 - 3, 0, (i / 4) * 2 - 3);
            }
            SetupTint();
            SetupDisplay();
        }

        Sprite CreateCircularGradientSprite(int index)
        {
            int textureSize = 256;
            Texture2D texture = new Texture2D(textureSize, textureSize);
            Color[] pixels = new Color[textureSize * textureSize];
            float outerRadius = textureSize / 2f; 
            float innerRadius = outerRadius * 0.7f;

            Color gradientColor = new Color(0.5f + index * 0.05f, 0, 0.5f, 1); 

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(outerRadius, outerRadius));
                    if (dist < innerRadius)
                    {
                        pixels[y * textureSize + x] = new Color(0, 0, 0, 0);
                    }
                    else if (dist < outerRadius)
                    {
                        float alpha = Mathf.Clamp01((dist - innerRadius) / (outerRadius - innerRadius));
                        pixels[y * textureSize + x] = new Color(gradientColor.r, gradientColor.g, gradientColor.b, alpha);
                    }
                    else
                    {
                        pixels[y * textureSize + x] = gradientColor;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }


        void SetupTint()
        {
            GameObject canvasObject = new GameObject("TintCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            tintObject = new GameObject("TintImage");
            tintImage = tintObject.AddComponent<Image>();
            tintImage.sprite = CreateCircularGradientSprite(0);
            tintImage.color = new Color(1, 1, 1, 0.1f);
            tintObject.transform.SetParent(canvasObject.transform);
            RectTransform rectTransform = tintObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        void SetupDisplay()
        {
            displayCanvas = new GameObject("DisplayCanvas");
            Canvas canvas = displayCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = displayCanvas.AddComponent<CanvasScaler>();
            scaler.scaleFactor = 1;
            displayCanvas.AddComponent<GraphicRaycaster>();

            for (int i = 0; i < 8; i++)
            {
                GameObject rawImageObject = new GameObject($"RawImage_{i}");
                RawImage rawImage = rawImageObject.AddComponent<RawImage>();
                rawImage.texture = renderTextures[i];

                RectTransform rectTransform = rawImageObject.GetComponent<RectTransform>();
                rectTransform.SetParent(displayCanvas.transform);
                rectTransform.anchorMin = new Vector2((i % 4) * 0.25f, (i / 4) * 0.5f);
                rectTransform.anchorMax = new Vector2((i % 4 + 1) * 0.25f, (i / 4 + 0.5f));
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                GameObject overlayImageObject = new GameObject($"OverlayImage_{i}");
                Image overlayImage = overlayImageObject.AddComponent<Image>();
                overlayImage.sprite = CreateCircularGradientSprite(i); 
                overlayImage.color = Color.white; 
                overlayImage.rectTransform.SetParent(rawImageObject.transform, false);
                overlayImage.rectTransform.anchorMin = Vector2.zero;
                overlayImage.rectTransform.anchorMax = Vector2.one;
                overlayImage.rectTransform.offsetMin = Vector2.zero;
                overlayImage.rectTransform.offsetMax = Vector2.zero;
            }

        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        void OnGameInitialized()
        {
            SetupCamera();
            Doug = GameObject.Find("PlumpBeetle");
        }

        void Update()
        {
            if (Doug != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (cameraProps[i] != null)
                    {
                        cameraProps[i].transform.position = Doug.transform.position;
                        cameraProps[i].transform.rotation = Doug.transform.rotation;
                    }
                }
            }
        }
    }
}
