using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XploriaAR
{
    [DisallowMultipleComponent, RequireComponent(typeof(RawImage))]
    public class UiWebCamera : MonoBehaviour
    {
        private RawImage rawImage;
        private WebCamTexture cameraTexture;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            cameraTexture = new WebCamTexture();
            rawImage.texture = cameraTexture;
            cameraTexture.Play();
        }
    }
}