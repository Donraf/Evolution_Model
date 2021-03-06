using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    private int amin_classes = 4;
    // TODO: Dict for genes (поставить в соответствие аллели определенным генам)
    private int genes_num = 4;
    private int triplet_size = 3;
    private int triplets_in_gene = 8;
    private int amins_in_gene;
    private Stack<Task> taskQueue = new Stack<Task>();
    public float health;
    public float velocity;
    public string sex;
    public string color;
    public char[] genes;
    public Dictionary<string, int> dict = new Dictionary<string, int>();
    public bool give_birth = true;
    private Material material;

    // Временный запрет на размножение при рождении
    public float birth_time;
    public float birth_pause = 4.0f;
    public class Task
    {
        public Task(string taskName, GameObject creature, float taskTime = 0.0f, float taskStartTime = 0.0f)
        {
            this.taskName = taskName;
            this.creature = creature;
            this.taskTime = taskTime;
            this.taskStartTime = taskStartTime;
        }
        public GameObject creature;
        public string taskName;
        public float taskTime;
        public float taskStartTime;

        public virtual bool action() { return false; }
    }

    private void Awake()
    {
        birth_time = Time.time;
    }

    void Start()
    {
        // Создание набора генов объекта
        amins_in_gene = triplet_size * triplets_in_gene;
        if (genes.Length == 0)
        {
            genes = new char[genes_num * amins_in_gene];
            int i = 0;
            foreach (char amin in genes)
            {
                genes[i] = (char)(97 + Mathf.FloorToInt(Random.Range(0.0f, amin_classes)));
                i++;
            }
        }
        // Создание словаря перевода триплетов в числа
        for (int j = 0; j < (int)Mathf.Pow(amin_classes, triplet_size); j++)
        {
            string key = "";
            for (int k = 0; k < triplet_size; k++)
            {
                key += (char)(97 + (int)(j / Mathf.Pow(amin_classes, k) % amin_classes));
            }
            dict.Add(key, j);
        }
        // Создание массива соответствия триплетов числовым значениям из словаря
        float[] arr_num = new float[genes.Length / triplet_size];
        for (int j = 0; j < genes.Length; j += triplet_size)
        {
            string key = "";
            for (int k = 0; k < triplet_size; k++)
            {
                key += genes[j + k];
            }
            arr_num[j / triplet_size] = dict[key];
        }
        // Нормализация координат вектора (1 вектор = 1 ген)
        for (int j = 0; j < arr_num.Length; j += triplets_in_gene)
        {
            float norm = 0.0f;
            for (int k = 0; k < triplets_in_gene; k++)
            {
                norm += Mathf.Pow(arr_num[j + k], 2);
            }
            norm = Mathf.Pow(norm, 0.5f);
            for (int k = 0; k < triplets_in_gene; k++)
            {
                arr_num[j + k] /= norm;
            }
        }
        // Подсчет здоровья и скорости
        float[] health_gene = getGene(0, arr_num);
        health = defineNumericCharacteristic(health_gene);
        float[] velocity_gene = getGene(1, arr_num);
        velocity = defineNumericCharacteristic(velocity_gene);
        float[] sex_gene = getGene(2, arr_num);
        sex = defineNumericCharacteristic(sex_gene) > 50 ? "Female" : "Male";
        float[] color_gene = getGene(3, arr_num);
        int color_num = defineNumericCharacteristic(color_gene);
        material = GetComponent<Renderer>().material;
        if (color_num < 56)
        {
            material.color = Color.black;
            color = "brown";
        }
        else if (color_num < 81)
        {
            material.color = Color.yellow;
            color = "yellow";
        }
        else if (color_num < 93)
        {
            material.color = Color.green;
            color = "orange";
        }
        else
        {
            material.color = Color.red;
            color = "red";
        }
    }

    void FixedUpdate()
    {
        Task taskToDo;
        if (taskQueue.Count == 0)
        {
            Vector3 step = new Vector3(Random.Range(-1.0f, 1.0f) * velocity * 0.01f, 0, Random.Range(-1.0f, 1.0f) * velocity * 0.01f);
            MoveTask new_task_move = new MoveTask(step ,"Move", gameObject, 0.6f);
            IdleTask new_task_idle = new IdleTask("Idle", gameObject, 0.1f);
            taskQueue.Push(new_task_idle);
            taskQueue.Push(new_task_move);
        }
        if (taskQueue.Count != 0)
        {
            taskToDo = taskQueue.Peek();
            doTask(ref taskToDo);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Creature") && collision.gameObject.GetComponent<Creature>().sex == "Male")
        {
            if (Time.time - birth_time > birth_pause && give_birth && sex == "Female")
            {
                createChild(gameObject, collision.gameObject);
                give_birth = false;
                collision.gameObject.GetComponent<Creature>().give_birth = false;
                Debug.Log("New Creature! Time:" + (Time.time - birth_time).ToString());
            }
        }
    }

    private void createChild(GameObject this_creature, GameObject another_creature)
    {
        GameObject child = Instantiate(this_creature, this_creature.transform.position + new Vector3(0, 20, 0), this_creature.transform.rotation);
        char[] childGenes = new char[genes_num * amins_in_gene];
        char[] fatherGenes = another_creature.GetComponent<Creature>().genes;

        if (amins_in_gene % 2 == 0)
        {
            for (int i = 0; i < genes.Length; i += amins_in_gene)
            {
                for (int j = 0; j < amins_in_gene / 2; j++)
                {
                    childGenes[i + j] = genes[i + j];
                    childGenes[i + j + amins_in_gene / 2] = fatherGenes[i + j + amins_in_gene / 2];
                }
            }
        }
        else
        {
            for (int i = 1; i < genes.Length; i += amins_in_gene)
            {
                for (int j = 0; j < amins_in_gene / 2; j++)
                {
                    childGenes[i + j] = genes[i + j];
                    childGenes[i + j + amins_in_gene / 2] = fatherGenes[i + j + amins_in_gene / 2];
                }
                childGenes[i - 1] = genes[i - 1];
            }
        }
        child.GetComponent<Creature>().genes = childGenes;
    }

    private int defineNumericCharacteristic(in float[] triplet_nums)
    {
        float x = 0.0f;
        for (int i = 0; i < triplet_nums.Length; i++)
        {
            x += i % 2 == 0 ? triplet_nums[i] : -triplet_nums[i];
        }
        int characteristic = (int)(1.0f / (1 + Mathf.Exp(-x * 3)) * 100);
        return characteristic;
    }

    private float[] getGene(in int allele, in float[] genes)
    {
        float[] gene = new float[triplets_in_gene];
        for (int i = 0; i < triplets_in_gene; i++)
        {
            gene[i] = genes[allele * triplets_in_gene + i];
        }
        return gene;
    }

    public void doTask(ref Task taskToDo)
    {
        if (!taskToDo.action())
        {
            taskQueue.Pop();
        }
    }

    public class MoveTask: Task
    {
        public Vector3 direction; 
        public MoveTask(Vector3 direction, string taskName, GameObject creature, float taskTime = 0.0f, float taskStartTime = 0.0f)
            : base(taskName, creature, taskTime, taskStartTime)
        {
            this.direction = direction;
        }

        public override bool action()
        {
            base.action();
            if (taskStartTime == 0.0f)
            {
                taskStartTime = Time.time;
            }
            if (Time.time - taskStartTime < taskTime)
            {
                //Vector3 step = new Vector3(Random.Range(-1.0f, 1.0f) * creature.GetComponent<Creature>().velocity * 0.01f, 0, Random.Range(-1.0f, 1.0f) * creature.GetComponent<Creature>().velocity * 0.01f);
                creature.transform.Translate(direction);
                return true;
            }
            return false;
        }
    }

    public class IdleTask : Task
    {
        public IdleTask(string taskName, GameObject creature, float taskTime = 0.0f, float taskStartTime = 0.0f)
            : base(taskName, creature, taskTime, taskStartTime) { }

        public override bool action()
        {
            base.action();
            if (taskStartTime == 0.0f)
            {
                taskStartTime = Time.time;
            }
            if (Time.time - taskStartTime < taskTime)
            {
                return true;
            }
            return false;
        }
    }

}
