using BookCloud.Models;

namespace BookCloud.Repositories.Interfaces
{
    public interface IRepositoryChats
    {
        Task<Chat?> ObtenerOCrearChatAsync(int usuario1Id, int usuario2Id);
        Task<List<Chat>> ObtenerChatsDeUsuarioAsync(int usuarioId);
        Task<Chat?> ObtenerChatPorIdAsync(int chatId);
        Task<List<Mensaje>> ObtenerMensajesDelChatAsync(int chatId, int take = 50);
        Task<Mensaje> EnviarMensajeAsync(int chatId, int remitenteId, string contenido);
        Task<bool> UsuarioPerteneceChatAsync(int chatId, int usuarioId);
    }
}