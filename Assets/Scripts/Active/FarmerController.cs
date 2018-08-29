using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class NetCacheEntry
{
    public string name;
    public string tag;
    public double fitness;
    public NeuralNet.Net net;

    public NetCacheEntry() { }

    public NetCacheEntry(string name, string tag, double fitness, NeuralNet.Net net)
    {
        this.name = name;
        this.tag = tag;
        this.fitness = fitness;
        this.net = net;
    }
}

public class FitnessComparer : IComparer<double>
{
    public int Compare(double x, double y)
    {
        int result = y.CompareTo(x);

        if (result == 0)
            return 1;
        else
            return result;
    }
}


public class FarmerController : MonoBehaviour
{
    private static FarmerController _instance = null;
    public static FarmerController Instance { get { return _instance; } }

    public int CropPopulation;
    public int HerbivorePopulation;
    public int CarnivorePopulation;

    public int SpeciesNetCacheSize;

    public Transform GrassParent;
    public Transform HerbivoreParent;
    public Transform CarnivoreParent;

    public GameObject GrassPrefab;
    public MovementNet HerbivorePrefab;
    public MovementNet CarnivorePrefab;

    private Vector3 fieldBounds;
    private string dataDirectory;
    private Dictionary<string, SortedList<double, NetCacheEntry>> fittestCache = new Dictionary<string, SortedList<double, NetCacheEntry>>();

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

    private SortedList<double, NetCacheEntry> getFittestList(string name)
    {
        if (fittestCache.ContainsKey(name))
        {
            return fittestCache[name];
        }
        else
        {
            SortedList<double, NetCacheEntry> list = new SortedList<double, NetCacheEntry>(new FitnessComparer());
            fittestCache[name] = list;
            return list;
        }
    }

    private bool addFittestList(NetCacheEntry entry)
    {
        if (entry.fitness <= 0)
        {
            return false;
        }

        SortedList<double, NetCacheEntry> fittestList = getFittestList(entry.name);

        if (fittestList.Count >= SpeciesNetCacheSize && fittestList.Count > 0 && fittestList.Last().Key >= entry.fitness)
        {
            // No better than anything in list
            return false;
        }

        bool fittest = false;
        if (fittestList.Count == 0 || (fittestList.Count > 0 && entry.fitness > fittestList.First().Key))
        {
            fittest = true;
            Debug.Log("Fittest " + entry.tag + ": " + entry.fitness);
        }
        fittestList.Add(entry.fitness, entry);
        while (fittestList.Count > SpeciesNetCacheSize)
        {
            fittestList.RemoveAt(SpeciesNetCacheSize);
        }
        return fittest;
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
            NetCacheEntry entry = new NetCacheEntry
            {
                name = name,
                tag = tag,
                net = net.Net,
                fitness = net.Net.Fitness
            };

            if (addFittestList(entry))
            {
                save();
            }
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
        createTemplatePrefabs();
        fill();
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

    void fillAnimal(Transform parent, int population, MovementNet animalPrefab, IEnumerable<NeuralNet.Net> fittestNets)
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
                if (fittestNets != null)
                {
                    nets.AddRange(fittestNets);
                }

                //if (parent.childCount > 0)
                //{
                //    Transform source = parent.GetChild(UnityEngine.Random.Range(0, parent.childCount));
                //    nets.Add(source.GetComponent<MovementNet>().Net);
                //}

                Debug.Log("POOL Size for " + animalPrefab.tag + ": " + nets.Count);
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

    private void createTemplatePrefabs()
    {
        CarnivorePrefab = Instantiate<MovementNet>(CarnivorePrefab);
        if (SimulationSettings.Instance)
        {
            CarnivorePrefab.hiddenLayers = SimulationSettings.Instance.CarnivoreLayers;
        }
        CarnivorePrefab.gameObject.SetActive(false);

        HerbivorePrefab = Instantiate<MovementNet>(HerbivorePrefab);
        if (SimulationSettings.Instance)
        {
            HerbivorePrefab.hiddenLayers = SimulationSettings.Instance.HerbivoreLayers;
        }
        HerbivorePrefab.gameObject.SetActive(false);
    }

    private void fill()
    {
        // Crops
        fillCrop(GrassParent, CropPopulation, GrassPrefab);

        // Carnivores 
        string type = String.Join(".", CarnivorePrefab.Net.layerSizes.Select(p => p.ToString()).ToArray());
        string name = CarnivorePrefab.tag + "-" + type;
        fillAnimal(
            CarnivoreParent, CarnivorePopulation, CarnivorePrefab,
            fittestCache.ContainsKey(name) ? fittestCache[name].Values.Select(entry => entry.net) : null
        );

        // Herbivores - Create template
        type = String.Join(".", HerbivorePrefab.Net.layerSizes.Select(p => p.ToString()).ToArray());
        name = HerbivorePrefab.tag + "-" + type;
        fillAnimal(
            HerbivoreParent, HerbivorePopulation, HerbivorePrefab,
            fittestCache.ContainsKey(name) ? fittestCache[name].Values.Select(entry => entry.net) : null
        );
    }

    private void load()
    {
        Debug.Log("Loading from: " + dataDirectory);
        IEnumerable<string> dataFiles = Directory.EnumerateFiles(dataDirectory, "*.json");

        foreach (string file in dataFiles)
        {
            NetSaveData save = JsonUtility.FromJson<NetSaveData>(File.ReadAllText(file));
            addFittestList(new NetCacheEntry(save.name, save.tag, save.fitness, new NeuralNet.FeedForward(save.layerSizes, save.weights)));
        }
    }

    private void save()
    {
        foreach (KeyValuePair<string, SortedList<double, NetCacheEntry>> entry in fittestCache)
        {
            if (entry.Value.Count == 0)
            {
                continue;
            }
            NetCacheEntry fittest = entry.Value.First().Value;
            string type = String.Join(".", ((NeuralNet.FeedForward)fittest.net).layerSizes.Select(p => p.ToString()).ToArray());

            string filename = fittest.name + ".json";

            // Save it
            NetSaveData save = new NetSaveData(fittest.name, fittest.tag, (NeuralNet.FeedForward)fittest.net);
            string savePath = Path.Combine(dataDirectory, filename);
            File.WriteAllText(savePath, JsonUtility.ToJson(save, true));
            Debug.Log("Saved as " + savePath);
        }
    }
}

[System.Serializable] 
public class NetSaveData
{
    public string name;
    public string tag;
    public double fitness;
    public int[] layerSizes;
    public double[] weights;

    public NetSaveData() { }

    public NetSaveData(string name, string tag, NeuralNet.FeedForward net)
    {
        this.name = name;
        this.tag = tag;
        this.fitness = net.Fitness;
        this.layerSizes = net.layerSizes;
        this.weights = net.weights;
    }
}