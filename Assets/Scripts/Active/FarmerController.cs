using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class NetCacheEntry
{
    public string tag;
    public double fitness;
    public NeuralNet.Net net;

    public NetCacheEntry() { }

    public NetCacheEntry(string tag, double fitness, NeuralNet.Net net)
    {
        this.tag = tag;
        this.fitness = fitness;
        this.net = net;
    }
}

public class FarmerController : MonoBehaviour
{
    private static FarmerController _instance = null;
    public static FarmerController Instance { get { return _instance; } }

    public int CropPopulation;
    public int HerbivorePopulation;
    public int CarnivorePopulation;

    public Transform GrassParent;
    public Transform HerbivoreParent;
    public Transform CarnivoreParent;

    public GameObject GrassPrefab;
    public MovementNet HerbivorePrefab;
    public MovementNet CarnivorePrefab;

    private Vector3 fieldBounds;
    private string dataDirectory;
    private Dictionary<string, NetCacheEntry> fittestCache = new Dictionary<string, NetCacheEntry>();

    private delegate List<NeuralNet.Net> mutator(List<NeuralNet.Net> nets);
    private List<mutator> Mutators = new List<mutator>()
    {
        (nets) => NeuralNet.Mutators.Clone(nets.ToArray()),
        (nets) => NeuralNet.Mutators.LayerCake(nets.Shuffle().ToArray()),
        (nets) => NeuralNet.Mutators.RandomMix(nets.ToArray()),
        (nets) => NeuralNet.Mutators.SelfMutate(nets.ToArray(), new NeuralNet.Options()
        {
            { "clone", true },
            { "mutationProbability", 0.1 },
            { "mutationFactor", 0.25 },
            { "mutationRange", 100 },
        })
    };
    private int mutatorIndex = 0;
    private List<NeuralNet.Net> NextMutator(List<NeuralNet.Net> nets)
    {
        mutatorIndex = (mutatorIndex + 1) % Mutators.Count;
        return Mutators[mutatorIndex](nets);
    }

    /**
     * When someone dies update the cache if they were the fittest.
     */
    public void OnDie(GameObject dead)
    {
        string tag = dead.tag;
        MovementNet net = dead.GetComponent<MovementNet>();
        

        if (net != null && net.Net != null && net.Net.Fitness > 0)
        {
            string type = String.Join(".", net.Net.layerSizes.Select(p => p.ToString()).ToArray());
            string name = tag + "-" + type;

            if (fittestCache.ContainsKey(name))
            {
                if (fittestCache[name].fitness >= net.Net.Fitness)
                {
                    // Not fitter
                    return;
                }
            }

            NetCacheEntry entry = new NetCacheEntry
            {
                tag = tag,
                net = net.Net,
                fitness = net.Net.Fitness
            };
            fittestCache[name] = entry;
            save();
            Debug.Log("FITEST " + tag + ": " + net.Net.Fitness);
        }
    }

    void Awake()
    {
        if (_instance != null)
        {
            throw new Exception("FarmerController is not null.");
        }
        _instance = this;
        dataDirectory = Application.persistentDataPath + "/data";
        Directory.CreateDirectory(dataDirectory);
        load();
    }

    void Start () {
        GameObject ground = GetComponent<WorldController>().Ground.gameObject;
        fieldBounds = ground.GetComponent<Renderer>().bounds.size;
        if (SimulationSettings.Instance)
        {
            CropPopulation = SimulationSettings.Instance.CropPopulation;
            CarnivorePopulation = SimulationSettings.Instance.CarnivorePopulation;
            HerbivorePopulation = SimulationSettings.Instance.HerbivorePopulation;
        }
        initializePopulation();
    }

    void OnDisable()
    {
        foreach (Transform child in CarnivoreParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in HerbivoreParent)
        {
            Destroy(child.gameObject);
        }

    }

    void Update()
    {
        fill();
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            HerbivorePopulation++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            HerbivorePopulation = Mathf.Max(0, --HerbivorePopulation);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            CarnivorePopulation++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadDivide))
        {
            CarnivorePopulation = Mathf.Max(0, --CarnivorePopulation);
        }
    }

    void fillAnimal(Transform parent, int population, MovementNet animalPrefab, NeuralNet.Net fittestNet)
    {
        if (parent.childCount < population)
        {
            while (parent.childCount < population)
            {
                Vector3 position = new Vector3(
                    (UnityEngine.Random.value - 0.5f) * fieldBounds.x * 0.9f,
                    GrassPrefab.transform.position.y,
                    (UnityEngine.Random.value - 0.5f) * fieldBounds.z * 0.9f);

                // TODO MOVE ALL THIS TO A BREEDER.....
                List<NeuralNet.Net> nets = new List<NeuralNet.Net>();
                if (fittestNet != null)
                {
                    nets.Add(fittestNet);
                }

                if (parent.childCount > 0)
                {
                    Transform source = parent.GetChild(UnityEngine.Random.Range(0, parent.childCount));
                    nets.Add(source.GetComponent<MovementNet>().Net);
                }

                if (nets.Count > 0)
                {
                    List<NeuralNet.Net> mutants = NextMutator(nets);
                    foreach (NeuralNet.Net net in mutants) {
                        MovementNet animal = Instantiate<MovementNet>(animalPrefab, position, Quaternion.identity, parent);
                        animal.TransplantNet(net);
                        animal.gameObject.SetActive(true);
                    }
                } else {
                    // Random
                    MovementNet animal = Instantiate<MovementNet>(animalPrefab, position, Quaternion.identity, parent);
                    animal.gameObject.SetActive(true);
                }
            }
        }
    }

    void fillCrop(Transform parent, int population, GameObject prefab)
    {
        while (parent.childCount < population)
        {
            Vector3 position = new Vector3(
                (UnityEngine.Random.value - 0.5f) * fieldBounds.x * 0.9f,
                GrassPrefab.transform.position.y,
                (UnityEngine.Random.value - 0.5f) * fieldBounds.z * 0.9f);
            Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }

    private void initializePopulation()
    {
        // Crops
        fillCrop(GrassParent, CropPopulation, GrassPrefab);

        // Carnivores - Create template
        CarnivorePrefab = Instantiate<MovementNet>(CarnivorePrefab);
        if (SimulationSettings.Instance)
        {
            CarnivorePrefab.hiddenLayers = SimulationSettings.Instance.CarnivoreLayers;
        }
        string type = String.Join(".", CarnivorePrefab.Net.layerSizes.Select(p => p.ToString()).ToArray());
        string name = CarnivorePrefab.tag + "-" + type;
        CarnivorePrefab.gameObject.SetActive(false);

        // Herbivores - Create template
        HerbivorePrefab = Instantiate<MovementNet>(HerbivorePrefab);
        if (SimulationSettings.Instance)
        {
            HerbivorePrefab.hiddenLayers = SimulationSettings.Instance.HerbivoreLayers;
        }
        type = String.Join(".", HerbivorePrefab.Net.layerSizes.Select(p => p.ToString()).ToArray());
        name = HerbivorePrefab.tag + "-" + type;
        HerbivorePrefab.gameObject.SetActive(false);

        // Fill them
        fillAnimal(
            CarnivoreParent, CarnivorePopulation, CarnivorePrefab,
             fittestCache.ContainsKey(name) ? fittestCache[name].net : null
        );

        fillAnimal(
            HerbivoreParent, HerbivorePopulation, HerbivorePrefab,
            fittestCache.ContainsKey(name) ? fittestCache[name].net : null
        );
    }

    private void fill()
    {
        // Crops
        fillCrop(GrassParent, CropPopulation, GrassPrefab);

        if (HerbivoreParent.childCount < HerbivorePopulation)
        {
            NeuralNet.Net fittestNet = fittestCache.ContainsKey("herbivore") ? fittestCache["herbivore"].net : null;
            fillAnimal(HerbivoreParent, HerbivorePopulation, HerbivorePrefab, fittestNet);
        }

        if (CarnivoreParent.childCount < CarnivorePopulation)
        {
            NeuralNet.Net fittestNet = fittestCache.ContainsKey("carnivore") ? fittestCache["carnivore"].net : null;
            fillAnimal(CarnivoreParent, CarnivorePopulation, CarnivorePrefab, fittestNet);
        }
    }

    private void load()
    {
        Debug.Log("Loading from: " + dataDirectory);
        IEnumerable<string> dataFiles = Directory.EnumerateFiles(dataDirectory, "*.json");

        foreach (string file in dataFiles)
        {            
            NetSaveData save = JsonUtility.FromJson<NetSaveData>(File.ReadAllText(file));
            fittestCache.Add(
                save.tag + "-" + String.Join(".", save.layerSizes.Select(p => p.ToString()).ToArray()),
                new NetCacheEntry(save.tag, save.fitness, new NeuralNet.FeedForward(save.layerSizes, save.weights))
            );
        }
    }

    private void save()
    {
        foreach (KeyValuePair<string, NetCacheEntry> entry in fittestCache)
        {
            string type = String.Join(".", ((NeuralNet.FeedForward)entry.Value.net).layerSizes.Select(p => p.ToString()).ToArray());

            string filename = Path.GetInvalidFileNameChars().Aggregate(
                entry.Value.tag + '-' + type,
                (current, c) => current.Replace(c, '_')) + ".json";

            // Save it
            NetSaveData save = new NetSaveData(entry.Value.tag, (NeuralNet.FeedForward)entry.Value.net);
            string savePath = Path.Combine(dataDirectory, filename);
            File.WriteAllText(savePath, JsonUtility.ToJson(save, true));
            Debug.Log("Saved as " + savePath);
        }
    }
}

[System.Serializable] 
public class NetSaveData
{
    public string tag;
    public double fitness;
    public int[] layerSizes;
    public double[] weights;

    public NetSaveData() { }

    public NetSaveData(string tag, NeuralNet.FeedForward net)
    {
        this.tag = tag;
        this.fitness = net.Fitness;
        this.layerSizes = net.layerSizes;
        this.weights = net.weights;
    }
}