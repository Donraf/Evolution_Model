using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    private int amin_classes = 4;
    // TODO: Dict for genes (поставить в соответствие аллели определенным генам)
    private int genes_num = 3;
    private int triplet_size = 3;
    private int triplets_in_gene = 4;
    private int amins_in_gene;
    public float health;
    public float velocity;
    public string sex;
    public char[] genes;
    public Dictionary<string, int> dict = new Dictionary<string, int>();
    public bool give_birth = true;
    // Временный запрет на размножение при рождении
    public float birth_time;
    public float birth_pause = 4.0f;

    private void Awake()
    {
        birth_time = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Создание набора генов объекта
        amins_in_gene = triplet_size * triplets_in_gene;
        genes = new char[genes_num * amins_in_gene];
        int i = 0;
        foreach (char amin in genes)
        {
            genes[i] = (char)(97 + Mathf.FloorToInt(Random.Range(0.0f, (float)amin_classes)));
            i++;
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
            arr_num[j / triplet_size] = (float)dict[key];
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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 step = new Vector3(Random.Range(-1.0f, 1.0f) * velocity * 0.01f, 0, Random.Range(-1.0f, 1.0f) * velocity * 0.01f);
        gameObject.transform.Translate(step);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Creature"))
        {
            //Debug.Log("Collision Time: " + Time.time.ToString() + " Birth Time: " + birth_time.ToString());
            if (Time.time - birth_time > birth_pause && give_birth)
            {
                createChild(gameObject, collision.gameObject);
                give_birth = false;
                Debug.Log("New Creature! Time:" + (Time.time - birth_time).ToString());
            }
        }
    }

    private void createChild(GameObject this_creature, GameObject another_creature)
    {
        Instantiate(this_creature, this_creature.transform.position + new Vector3(0, 20, 0), this_creature.transform.rotation);
    }

    // TODO: Передача по ссылке
    private int defineNumericCharacteristic(float[] triplet_nums)
    {
        float x = 0.0f;
        for (int i = 0; i < triplet_nums.Length; i++)
        {
            x += i % 2 == 0 ? triplet_nums[i] : -triplet_nums[i];
        }
        int characteristic = (int)(1.0f / (1 + Mathf.Exp(-x * 3)) * 100);
        return characteristic;
    }

    // TODO: Передача по ссылке
    private float[] getGene(int allele, float[] genes)
    {
        float[] gene = new float[triplets_in_gene];
        for (int i = 0; i < triplets_in_gene; i++)
        {
            gene[i] = genes[allele * triplets_in_gene + i];
        }
        return gene;
    }
}
