namespace OllamaYarpProject;

public class ModelData
{
    public string id { get; set; }
    public string @object { get; set; }
    public long created { get; set; }
    public string owned_by { get; set; }
}

public class SourceRoot
{
    public List<ModelData> data { get; set; }
    public string @object { get; set; }
}

public class OllamaModelDetails
{
    public string parent_model { get; set; } = "";
    public string format { get; set; } = "gguf";
    public string family { get; set; }
    public List<string> families { get; set; }
    public string parameter_size { get; set; } = "3B";
    public string quantization_level { get; set; } = "Q5_K_M";
}

public class OllamaModel
{
    public string name { get; set; }
    public string model { get; set; }
    public string modified_at { get; set; }
    public long size { get; set; }
    public string digest { get; set; }
    public OllamaModelDetails details { get; set; }
}

public class OllamaRoot
{
    public List<OllamaModel> models { get; set; }
}