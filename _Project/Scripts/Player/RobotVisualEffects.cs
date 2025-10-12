using UnityEngine;

/// <summary>
/// Визуальные эффекты для одноколесного робота
/// </summary>
public class RobotVisualEffects : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer wheelTrail;
    [SerializeField] private float trailLength = 0.5f;
    [SerializeField] private Gradient trailGradient;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private ParticleSystem sparksParticles;
    [SerializeField] private float dustEmissionRate = 10f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource wheelAudioSource;
    [SerializeField] private AudioClip wheelRollSound;
    [SerializeField] private AudioClip balanceSound;
    [SerializeField] private float pitchRange = 0.5f;
    
    private UnicycleRobotController robotController;
    private BeltMover beltMover;
    private float lastSpeed;
    private bool isPlayingWheelSound;

    private void Awake()
    {
        robotController = GetComponent<UnicycleRobotController>();
        beltMover = GetComponent<BeltMover>();
        
        SetupTrail();
        SetupParticles();
        SetupAudio();
    }

    private void Start()
    {
        // Подписываемся на события робота
        if (robotController != null)
        {
            robotController.OnWheelSpin += OnWheelSpin;
            robotController.OnTiltChanged += OnTiltChanged;
            robotController.OnBalanceLost += OnBalanceLost;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (robotController != null)
        {
            robotController.OnWheelSpin -= OnWheelSpin;
            robotController.OnTiltChanged -= OnTiltChanged;
            robotController.OnBalanceLost -= OnBalanceLost;
        }
    }

    private void Update()
    {
        UpdateTrail();
        UpdateParticles();
        UpdateAudio();
    }

    /// <summary>
    /// Настраивает след от колеса
    /// </summary>
    private void SetupTrail()
    {
        if (wheelTrail != null)
        {
            wheelTrail.time = trailLength;
            wheelTrail.colorGradient = trailGradient;
            wheelTrail.enabled = false;
        }
    }

    /// <summary>
    /// Настраивает частицы
    /// </summary>
    private void SetupParticles()
    {
        if (dustParticles != null)
        {
            var emission = dustParticles.emission;
            emission.enabled = false;
        }
        
        if (sparksParticles != null)
        {
            var emission = sparksParticles.emission;
            emission.enabled = false;
        }
    }

    /// <summary>
    /// Настраивает аудио
    /// </summary>
    private void SetupAudio()
    {
        if (wheelAudioSource != null && wheelRollSound != null)
        {
            wheelAudioSource.clip = wheelRollSound;
            wheelAudioSource.loop = true;
            wheelAudioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Обновляет след от колеса
    /// </summary>
    private void UpdateTrail()
    {
        if (wheelTrail != null)
        {
            float currentSpeed = beltMover.GetCurrentSpeed();
            bool shouldShowTrail = currentSpeed > 0.5f;
            
            if (wheelTrail.enabled != shouldShowTrail)
            {
                wheelTrail.enabled = shouldShowTrail;
            }
        }
    }

    /// <summary>
    /// Обновляет частицы
    /// </summary>
    private void UpdateParticles()
    {
        if (dustParticles != null)
        {
            float currentSpeed = beltMover.GetCurrentSpeed();
            var emission = dustParticles.emission;
            
            if (currentSpeed > 1f)
            {
                emission.enabled = true;
                emission.rateOverTime = dustEmissionRate * currentSpeed;
            }
            else
            {
                emission.enabled = false;
            }
        }
    }

    /// <summary>
    /// Обновляет аудио
    /// </summary>
    private void UpdateAudio()
    {
        if (wheelAudioSource != null && wheelRollSound != null)
        {
            float currentSpeed = beltMover.GetCurrentSpeed();
            bool shouldPlay = currentSpeed > 0.1f;
            
            if (shouldPlay && !isPlayingWheelSound)
            {
                wheelAudioSource.Play();
                isPlayingWheelSound = true;
            }
            else if (!shouldPlay && isPlayingWheelSound)
            {
                wheelAudioSource.Stop();
                isPlayingWheelSound = false;
            }
            
            if (isPlayingWheelSound)
            {
                // Изменяем высоту тона в зависимости от скорости
                float pitch = 1f + (currentSpeed / 4f) * pitchRange;
                wheelAudioSource.pitch = pitch;
            }
        }
    }

    /// <summary>
    /// Обработчик вращения колеса
    /// </summary>
    /// <param name="angularVelocity">Угловая скорость</param>
    private void OnWheelSpin(float angularVelocity)
    {
        // Можно добавить дополнительные эффекты при вращении
    }

    /// <summary>
    /// Обработчик изменения наклона
    /// </summary>
    /// <param name="tilt">Угол наклона</param>
    private void OnTiltChanged(float tilt)
    {
        // Эффекты при наклоне
        if (Mathf.Abs(tilt) > 20f && sparksParticles != null)
        {
            var emission = sparksParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = Mathf.Abs(tilt) * 2f;
        }
        else if (sparksParticles != null)
        {
            var emission = sparksParticles.emission;
            emission.enabled = false;
        }
    }

    /// <summary>
    /// Обработчик потери баланса
    /// </summary>
    private void OnBalanceLost()
    {
        // Эффекты при потере баланса
        if (sparksParticles != null)
        {
            var emission = sparksParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 50f;
        }
        
        if (wheelAudioSource != null && balanceSound != null)
        {
            wheelAudioSource.PlayOneShot(balanceSound);
        }
    }

    /// <summary>
    /// Устанавливает настройки следа
    /// </summary>
    /// <param name="length">Длина следа</param>
    /// <param name="gradient">Градиент цвета</param>
    public void SetTrailSettings(float length, Gradient gradient)
    {
        trailLength = length;
        trailGradient = gradient;
        
        if (wheelTrail != null)
        {
            wheelTrail.time = length;
            wheelTrail.colorGradient = gradient;
        }
    }

    /// <summary>
    /// Устанавливает настройки частиц
    /// </summary>
    /// <param name="dustRate">Скорость эмиссии пыли</param>
    public void SetParticleSettings(float dustRate)
    {
        dustEmissionRate = dustRate;
    }
}


