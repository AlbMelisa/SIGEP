import { useState } from 'react';
import { LogIn, X, LoaderCircle, ShieldAlert } from 'lucide-react';
import { useAuth } from '../context/AuthContext.jsx';

export function LoginModal({ isOpen, onClose }) {
  const { login, loading, authError, setAuthError } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!username.trim() || !password) {
      setAuthError('Por favor complete todos los campos');
      return;
    }
    const res = await login(username, password);
    if (res.success) {
      onClose();
    }
  };

  return (
    <div className="modal-scrim" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '420px' }}>
        <div className="modal-heading">
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <div className="workflow-icon" style={{ width: '32px', height: '32px' }}>
              <LogIn size={16} />
            </div>
            <div>
              <h2 style={{ fontSize: '18px', margin: 0 }}>Iniciar sesión</h2>
              <span style={{ fontSize: '10px', color: '#687f71' }}>SIGEP - Municipalidad de Simoca</span>
            </div>
          </div>
          <button
            type="button"
            className="pagination-button"
            onClick={onClose}
            aria-label="Cerrar"
          >
            <X size={15} />
          </button>
        </div>

        {authError && (
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '10px 12px',
              marginBottom: '15px',
              borderRadius: '5px',
              backgroundColor: '#fbefed',
              color: '#b65d50',
              fontSize: '11px',
              border: '1px solid #f5cfc9',
            }}
          >
            <ShieldAlert size={16} />
            <span>{authError}</span>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-grid" style={{ gridTemplateColumns: '1fr' }}>
            <label className="form-wide">
              Usuario
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Ej: jefe_personal o encargado"
                autoFocus
                required
              />
            </label>

            <label className="form-wide">
              Contraseña
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                required
              />
            </label>

            <div className="modal-actions" style={{ marginTop: '15px' }}>
              <button
                type="button"
                className="button button-outline"
                onClick={onClose}
                disabled={loading}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="button button-primary"
                disabled={loading}
                style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}
              >
                {loading && <LoaderCircle size={14} className="loading-spinner" />}
                {loading ? 'Identificando...' : 'Acceder al sistema'}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
}

