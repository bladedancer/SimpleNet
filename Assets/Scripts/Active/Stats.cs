using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Stats : MonoBehaviour {
    public string[] Menu;

    public float MaxHealth;
    public float MaxMovementSpeed;
    public float MaxRotationSpeed;
    public float MaxNutrition;
    public float HealthDecayRate;

    [SerializeField]
    private float _health;

    public float Age { get; private set; }
    public float Health { get { return _health; }}
    public float MovementSpeed { get; private set; }
    public float RotationSpeed { get; private set; }
    public float Nutrition { get; private set; }

    // Amount eaten by this guy
    public float Consumed { get; private set; }

    public delegate void ChangeValue(float value);
    public delegate void ChangeValueCapped(float value, float max);

    public ChangeValue OnChangeConsumed = delegate { };
    public ChangeValueCapped OnChangeHealth = delegate { };

    void Start()
    {
        Age = 0;
        _health = MaxHealth;
        MovementSpeed = MaxMovementSpeed;
        RotationSpeed = MaxRotationSpeed;
        Nutrition = MaxNutrition;
    }
    
    public void AddHealth(float health)
    {
        _health = Mathf.Min(Health + health, MaxHealth);
        Consumed += health;
        OnChangeConsumed(Consumed);
        OnChangeHealth(Health, MaxHealth);
    }

	// Update is called once per frame
	void FixedUpdate() {
        Age += Time.deltaTime;

	    if (_health <= 0)
        {
            Destroy(gameObject);
        }

        if (_health < MaxHealth / 4)
        {
            float multiplier = (_health / (MaxHealth / 4));
            Nutrition = MaxNutrition * multiplier;

        }

        // Might make this smarter
        if (_health < MaxHealth / 2)
        {
            float multiplier = (_health / (MaxHealth / 2));
            MovementSpeed = MaxMovementSpeed * multiplier;
            RotationSpeed = MaxRotationSpeed * multiplier;
        }

        _health -= HealthDecayRate * Time.deltaTime;
        OnChangeHealth(Health, MaxHealth);
    }
}
