using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SwiftUpdate.Models;

namespace SwiftUpdate.Services
{
    public class SessionService
    {
        private readonly SwiftUpdateContext _context;

        public SessionService(SwiftUpdateContext context)
        {
            _context = context;
        }

        // Create a new session
        public string CreateSession(string sessionGuid, int userId, DateTime expiryTime)
        {
            var session = new SessionModel
            {
                SessionGuid = sessionGuid,
                UserId = userId,
                ExpiryTime = expiryTime,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Sessions.Add(session);
            _context.SaveChanges();


       
            return session.SessionGuid ?? string.Empty;
        }

        // Retrieve a session by session ID
        public SessionModel GetSessionById(int sessionId)
        {
            return _context.Sessions
                .Include(s => s.User) // Include associated user
                .FirstOrDefault(s => s.SessionId == sessionId);
        }
        public SessionModel GetSessionByGuid(string sessionGuid)
        {
            return _context.Sessions
                .Include(s => s.User) // Include associated user
                .FirstOrDefault(s => s.SessionGuid == sessionGuid);
        }
        // Update an existing session
        public void UpdateSession(SessionModel session)
        {
            session.UpdatedAt = DateTime.Now;
            _context.Sessions.Update(session);
            _context.SaveChanges();
        }

        // Delete a session by session ID
        public void DeleteSession(int sessionId)
        {
            var session = _context.Sessions.Find(sessionId);
            if (session != null)
            {
                _context.Sessions.Remove(session);
                _context.SaveChanges();
            }
        }


        public void DeleteSessionByGuid(string Guid)
        {
            var session = _context.Sessions.Where(
                x => x.SessionGuid == Guid
            ).FirstOrDefault();

            if (session != null)
            {
                _context.Sessions.Remove(session);
                _context.SaveChanges();
            }
        }


        // Check if a session exists
        public bool SessionExists(int sessionId)
        {
            return _context.Sessions.Any(s => s.SessionId == sessionId);
        }

        // Check if a session is expired
        public bool IsSessionExpired(int sessionId)
        {
            var session = _context.Sessions.Find(sessionId);
            if (session == null)
            {
                throw new ArgumentException("Session not found.");
            }

            return session.ExpiryTime < DateTime.Now;
        }

        // Extend session expiry time
        public void ExtendSession(int sessionId, DateTime newExpiryTime)
        {
            var session = _context.Sessions.Find(sessionId);
            if (session == null)
            {
                throw new ArgumentException("Session not found.");
            }

            session.ExpiryTime = newExpiryTime;
            session.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }
}
