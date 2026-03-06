using BookCloud.Data;
using BookCloud.Models;
using BookCloud.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCloud.Repositories
{
    public class RepositoryChats : IRepositoryChats
    {
        private readonly BookCloudContext _context;

        public RepositoryChats(BookCloudContext context)
        {
            _context = context;
        }

        public async Task<Chat?> ObtenerOCrearChatAsync(int usuario1Id, int usuario2Id)
        {
            // Buscar chat existente usando los índices IX_Chats_Usuario1Id y IX_Chats_Usuario2Id
            var chat = await _context.Chats
                .Where(c => c.Activo &&
                    ((c.Usuario1Id == usuario1Id && c.Usuario2Id == usuario2Id) ||
                     (c.Usuario1Id == usuario2Id && c.Usuario2Id == usuario1Id)))
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                // Crear nuevo chat
                chat = new Chat
                {
                    Usuario1Id = usuario1Id,
                    Usuario2Id = usuario2Id,
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }

            return chat;
        }

        public async Task<List<Chat>> ObtenerChatsDeUsuarioAsync(int usuarioId)
        {
            // Usa los índices IX_Chats_Usuario1Id, IX_Chats_Usuario2Id y IX_Chats_Activo
            return await _context.Chats
                .Include(c => c.Usuario1)
                .Include(c => c.Usuario2)
                .Where(c => c.Activo && (c.Usuario1Id == usuarioId || c.Usuario2Id == usuarioId))
                .Select(c => new Chat
                {
                    Id = c.Id,
                    Usuario1Id = c.Usuario1Id,
                    Usuario2Id = c.Usuario2Id,
                    FechaCreacion = c.FechaCreacion,
                    Activo = c.Activo,
                    Usuario1 = c.Usuario1,
                    Usuario2 = c.Usuario2,
                    Mensajes = c.Mensajes
                        .Where(m => m.Activo)
                        .OrderByDescending(m => m.FechaEnvio)
                        .Take(1)
                        .ToList()
                })
                .OrderByDescending(c => c.Mensajes.Any() ? c.Mensajes.Max(m => m.FechaEnvio) : c.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Chat?> ObtenerChatPorIdAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Usuario1)
                .Include(c => c.Usuario2)
                .FirstOrDefaultAsync(c => c.Id == chatId && c.Activo);
        }

        public async Task<List<Mensaje>> ObtenerMensajesDelChatAsync(int chatId, int take = 50)
        {
            // Usa los índices IX_Mensajes_ChatId y IX_Mensajes_Activo
            return await _context.Mensajes
                .Include(m => m.Remitente)
                .Where(m => m.ChatId == chatId && m.Activo)
                .OrderByDescending(m => m.FechaEnvio)
                .Take(take)
                .OrderBy(m => m.FechaEnvio) // Reordenar para mostrar cronológicamente
                .ToListAsync();
        }

        public async Task<Mensaje> EnviarMensajeAsync(int chatId, int remitenteId, string contenido)
        {
            var mensaje = new Mensaje
            {
                ChatId = chatId,
                RemitenteId = remitenteId,
                Contenido = contenido,
                FechaEnvio = DateTime.Now,
                Activo = true
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();

            // Cargar el remitente para devolverlo completo
            await _context.Entry(mensaje)
                .Reference(m => m.Remitente)
                .LoadAsync();

            return mensaje;
        }

        public async Task<bool> UsuarioPerteneceChatAsync(int chatId, int usuarioId)
        {
            return await _context.Chats
                .AnyAsync(c => c.Id == chatId &&
                    c.Activo &&
                    (c.Usuario1Id == usuarioId || c.Usuario2Id == usuarioId));
        }
    }
}