using Microsoft.VisualStudio.Web.CodeGeneration.Templating;

namespace UVM1._5.Models
{

    public class VisionRequest
    {
        public string model { get; } = "gpt-4-vision-preview";
        public Message[] messages { get; set; }
        public int max_tokens { get; } = 300;

        public VisionRequest(string _text, string _url)
        {
            messages = new Message[1];
            messages[0] = new Message(_url, _text);
        }
    }

    public class Message
    {
        public string role { get; } = "user";
        public List<dynamic> content { get; set; }

        public Message(string _url, string _text)
        {
            content = new List<dynamic>();
            content.Add(new TextContent(_text));
            content.Add(new ImageContent(_url));
        }
    }

    public class Content
    {
        public string type { get; set; }
        
        

        
    }

    public class ImageContent : Content
    {
        
        public Image_Url image_url { get; set; }

        public ImageContent(string _url)
        {
            type = "image_url";
            image_url = new Image_Url(_url);
            
        }

    }

    public class TextContent : Content
    {
        public string text { get; set; }

        public TextContent(string _text)
        {
            type = "text";
            text = _text;


        }
    }

    public class Image_Url
    {
        public string url { get; set; }

        public Image_Url(string _url)
        {
            url = _url;
        }
    }



}
