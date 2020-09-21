using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine.UI;

public class TableController : MonoBehaviour
{
    /// <summary>
    /// Nombres de las variables que definen los parametros de la tabla
    /// </summary>
    public string varNameColumns, varNameTitle, varNameData, filename;

    /// <summary>
    /// Prefabs de UI de las partes de la tabla
    /// </summary>
    public GameObject rowPrefab, cellPrefab;

    /// <summary>
    /// Objeto padre que arma nuestra tabla
    /// </summary>
    public GameObject tableParent;

    /// <summary>
    /// Nombres de las columnas para saber si nuestros datos les faltan parametros.
    /// </summary>
    List<string> columnNames;

    bool fileChanged = false;

    FileSystemWatcher watcher;

    // Iniciar el vigilante de cambios
    private void OnEnable()
    {
        watcher = new FileSystemWatcher();      
        watcher.Path = Application.streamingAssetsPath;
        watcher.Filter = filename;

        watcher.NotifyFilter = NotifyFilters.LastWrite;

        // Add event handlers
        watcher.Changed += OnChanged;

        // Begin watching
        watcher.EnableRaisingEvents = true;
    }

    // Eliminar el vigilante
    private void OnDisable()
    {
        if (watcher != null)
        {
            watcher.Changed -= OnChanged;
            watcher.Dispose();
        }
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
        fileChanged = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadTable();
    }

    private void Update()
    {
        if (fileChanged)
        {
            // Do something here…
            fileChanged = false;
            LoadTable();
        }
    }


    void LoadTable()
    {
        if (File.Exists(Path.Combine(Application.streamingAssetsPath, filename)))   //Si el JSON existe
        {
            CleanTable();       //Limpiar la tabla

            string jsonText = File.OpenText(Path.Combine(Application.streamingAssetsPath, filename)).ReadToEnd();   //Obtener el texto
            var jsonObject = JObject.Parse(jsonText);
            string tableName = (string)jsonObject[varNameTitle];        //Obtenemos los datos de la tabla

            columnNames = jsonObject[varNameColumns].ToObject<List<string>>();

            GameObject currRow = Instantiate(rowPrefab, tableParent.transform);
            Instantiate(cellPrefab, currRow.transform).GetComponent<Text>().text = "<b>" + tableName + "</b>";

            currRow = Instantiate(rowPrefab, tableParent.transform);

            foreach (string column in columnNames)
            {
                Instantiate(cellPrefab, currRow.transform).GetComponent<Text>().text = "<b>" + column + "</b>";
            }

            JArray arrayData = (JArray)jsonObject[varNameData];

            foreach (JObject item in arrayData)
            {
                currRow = Instantiate(rowPrefab, tableParent.transform);

                foreach (string column in columnNames)
                {
                    var prop = item.GetValue(column);
                    if (prop == null)
                        Instantiate(cellPrefab, currRow.transform).GetComponent<Text>().text = "<i>N/A</i>";
                    else
                        Instantiate(cellPrefab, currRow.transform).GetComponent<Text>().text = prop.ToString();
                }
            }

        }
    }

    void CleanTable()
    {
        foreach (Transform child in tableParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
