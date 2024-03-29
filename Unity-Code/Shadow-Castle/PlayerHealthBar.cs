using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Sprite[] heartIcons;
    [SerializeField] private GameObject heartPrefab;
    
    private List<Image> _heartImages;
    
    void Awake()
    {
        _heartImages = GetComponentsInChildren<Image>().ToList();
    }

    public void UpdateHealth(int health)
    {
        int healthLeft = health;
        foreach (Image heart in _heartImages)
        {
            heart.sprite = heartIcons[Math.Min(4, healthLeft)];
            healthLeft = Mathf.Max(0, healthLeft - 4);
        }
    }

    public void AddHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab, transform);
        _heartImages.Add(newHeart.GetComponent<Image>());
    }
}
