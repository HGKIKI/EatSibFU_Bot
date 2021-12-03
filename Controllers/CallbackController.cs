using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Model.Keyboard;
using System.Drawing;
using VkNet.Enums.SafetyEnums;
using DAL1;


namespace EatSibFU_Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        public void SendMessage(long? PeerId, string Text, MessageKeyboard keyboard = null)
        {

            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new Random().Next(999999),
                PeerId = PeerId,
                Message = Text,
                Keyboard = keyboard
            });
        }

        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;

        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {



            DAL D = new DAL();
            // Проверяем, что находится в поле "type" 

            switch (updates.Type)
            {

                case "message_new":
                    {

                        // Десериализация

                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        if (msg.Payload != null)
                        {
                            switch (msg.Text)
                            {

                                case "Начать":
                                    {

                                        KeyboardBuilder key = new KeyboardBuilder(isOneTime: false);
                                        key.AddButton("Корзина &#128722;", "");
                                        key.AddButton("Выбрать буфет &#127860;", "");
                                        MessageKeyboard keyboard = key.Build();
                                        SendMessage(msg.PeerId, "Привет! Проголодался?", keyboard);
                                        break;
                                    }
                                case "Выбрать буфет 🍴":
                                    {
                                        var CanteenList = D.GetCanteen().Result;
                                        KeyboardBuilder key = new KeyboardBuilder(isOneTime: false);
                                        key.AddButton("1. Буфет ИКИТ", "", KeyboardButtonColor.Positive).AddLine();
                                        key.AddButton("2. Буфет ПИ", "", KeyboardButtonColor.Positive).AddLine();
                                        key.AddButton("3. Буфет ИНиГ", "", KeyboardButtonColor.Positive).AddLine();
                                        MessageKeyboard keyboard = key.Build();
                                        string str = "";
                                        int count = 1;
                                        foreach (Canteen canteen in CanteenList)
                                        {
                                            str += $"{count++}. {canteen.Name}\n";
                                        }
                                        SendMessage(msg.PeerId, str, keyboard);
                                        break;
                                    }
                                case "1. Буфет ИКИТ":
                                    {
                                        D.ChooseCanteen("Буфет ИКИТ", msg.PeerId);
                                      
                                        break;
                                    }
                                case "2. Буфет ПИ":
                                    {
                                        D.ChooseCanteen("Буфет ПИ", msg.PeerId);
                                        break;
                                    }
                                case "3. Буфет ИНиГ":
                                    {
                                        D.ChooseCanteen("Буфет ИНиГ", msg.PeerId);
                                        break;
                                    }


                            }
                        }
                        else
                            SendMessage(msg.PeerId, "У тебя нет свободы выборы. Кнопочки.");
                        break;
                    }
                // Если это уведомление для подтверждения адреса
                case "confirmation":
                    // Отправляем строку для подтверждения 
                    return Ok(_configuration["Config:Confirmation"]);


            }
            // Возвращаем "ok" серверу Callback API
            return Ok("ok");
        }
    }
}