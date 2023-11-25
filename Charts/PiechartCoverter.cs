

//Converts our data to pie charts
using System.Drawing;


public class Piechart<T>{

    private readonly int[] dimensions = {512, 512};
    private readonly Point center = new Point(256, 256);
    private const int radius = 128;

    public const string file_name = "chart.png";

    private Color[] colors;
    private List<T> data; //Data is going to be one of our enums
    private Dictionary<T, long> appearances;

    private long sum = 0;

    public Piechart(List<T> data, Color[] colors){
        this.data = data;
        this.colors = colors;
        appearances = new Dictionary<T, long>();
        foreach(T data_point in data){
            try{
                appearances[data_point]++;
            }catch(Exception e){
                appearances.Add(data_point, 1);
            }
        }
    }

    public string GetKey(){
        string key = "Here's a key for your chart!";

        int iter = 0;
        foreach(KeyValuePair<T, long> pair in appearances){
            try{
                if(pair.Key is Gender gender){
                    key += "\n" + $"{colors[iter]}: {Cat.GetGenderString(gender)} ({(pair.Value/(double)sum) * 100:F2}%)";
                } else
                if(pair.Key is Clan clan) {
                    key += "\n" + $"{colors[iter]}: {Cat.GetClanString(clan)} ({(pair.Value/(double)sum) * 100:F2}%)";
                }else 
                if(pair.Key is Rank rank){
                    key += "\n" + $"{colors[iter]}: {Cat.GetRankString(rank)} ({(pair.Value/(double)sum) * 100:F2}%)";
                }
            }catch(Exception e){
                return "Problem getting key";
            }
            iter++;
        }

        return key;
    }

    public Bitmap BuildChart(){
        Bitmap image = new Bitmap(dimensions[0], dimensions[1]);

        try{
            
            using(Graphics g = Graphics.FromImage(image)){
                CreatePiechart(g);
            }
        }catch(Exception e){
            Logger.Error(e.Message +" ERROR NO: 002");
        }

        return image;
    }

    public void CreatePiechart(Graphics g){
        long total = 0;
        foreach(KeyValuePair<T, long> key_pair in appearances){
            total += key_pair.Value;
        }
        sum = total;

        double start_angle = 0;

        int iter = 0;
        try{
            foreach(KeyValuePair<T, long> key_pair in appearances){
                double sweep_angle = 360.0 * key_pair.Value/total;

                using(Brush brush = new SolidBrush(colors[iter])){
                    g.FillPie(brush, center.X - radius, center.Y - radius, 2 * radius, 2 * radius, (float)start_angle, (float)sweep_angle);
                }        
                start_angle += sweep_angle;
                iter++;
            }
        }catch(Exception e){
            Logger.Error(e.Message + " ERROR NO: 001");
        }
        
    }

    public static void SaveChart(Bitmap chart){
        File.Delete(file_name);
        chart.Save(file_name, System.Drawing.Imaging.ImageFormat.Png);
    }
    

}