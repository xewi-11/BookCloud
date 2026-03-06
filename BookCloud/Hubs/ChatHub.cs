using BookCloud.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BookCloud.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IRepositoryChats _repositoryChats;

        public ChatHub(IRepositoryChats repositoryChats)
        {
            _repositoryChats = repositoryChats;
        }

        public async Task EnviarMensaje(int chatId, int usuarioId, string contenido)
        {
            try
            {
                // Verificar que el usuario pertenezca al chat
                if (!await _repositoryChats.UsuarioPerteneceChatAsync(chatId, usuarioId))
                {
                    throw new HubException("No tienes permiso para enviar mensajes en este chat");
                }

                // Guardar mensaje en BD
                var mensaje = await _repositoryChats.EnviarMensajeAsync(chatId, usuarioId, contenido);

                // Enviar mensaje a todos los clientes conectados a este chat
                await Clients.Group($"Chat_{chatId}").SendAsync("RecibirMensaje", new
                {
                    id = mensaje.Id,
                    chatId = mensaje.ChatId,
                    remitenteId = mensaje.RemitenteId,
                    remitenteNombre = mensaje.Remitente?.Nombre ?? "Usuario",
                    contenido = mensaje.Contenido,
                    fechaEnvio = mensaje.FechaEnvio.ToString("HH:mm")
                });
            }
            catch (Exception ex)
            {
                throw new HubException($"Error al enviar mensaje: {ex.Message}");
            }
        }

        public async Task UnirseAlChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        }

        public async Task SalirDelChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        }
    }
}