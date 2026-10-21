import { useState } from 'react'
import { KeyRound, LoaderCircle, ShieldAlert, UserRound } from 'lucide-react'
import { useAuth } from '../context/AuthContext.jsx'

export function LoginPage() {
  const { login, loading, authError, setAuthError } = useAuth()
  const [username, setUsername] = useState('jefe_admin')
  const [password, setPassword] = useState('Password123!')
  const [rememberMe, setRememberMe] = useState(true)

  const handleSubmit = async (event) => {
    event.preventDefault()
    if (!username.trim() || !password) {
      setAuthError('Ingresá tu usuario y contraseña para continuar.')
      return
    }
    await login(username.trim(), password, rememberMe)
  }

  return <main className="login-screen">
    <div className="login-particles" aria-hidden="true">
      {[
        [8, 18, 0], [17, 74, 2.1], [25, 36, 3.7], [34, 87, 1.2],
        [43, 13, 4.6], [52, 65, 2.8], [61, 28, 5.3], [69, 82, .7],
        [77, 45, 3.2], [86, 16, 1.8], [93, 69, 4.1], [12, 51, 5.7],
        [38, 57, .3], [72, 12, 2.5], [90, 91, 4.9], [5, 90, 1.5],
      ].map(([left, top, delay], index) => <span key={index} style={{ left: `${left}%`, top: `${top}%`, animationDelay: `${delay}s` }} />)}
    </div>
    <section className="login-card" aria-labelledby="login-title">
      <img className="login-logo" src="/sigep-logo-transparente.png" alt="SIGEP · Sistema Integral de Gestión de Eventos y Personal · Municipalidad de Simoca" />
      <h1 id="login-title">Iniciar sesión</h1>
      <p className="login-subtitle">Ingresá para continuar al sistema de gestión.</p>

      {authError && <div className="login-error" role="alert"><ShieldAlert size={17} /><span>{authError}</span></div>}

      <form className="login-form" onSubmit={handleSubmit}>
        <label className="login-field">
          <span>Usuario</span>
          <span className="login-input-wrap"><UserRound size={17} /><input autoComplete="username" autoFocus value={username} onChange={(event) => setUsername(event.target.value)} placeholder="Tu usuario" required /></span>
        </label>
        <label className="login-field">
          <span>Contraseña</span>
          <span className="login-input-wrap"><KeyRound size={17} /><input type="password" autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Tu contraseña" required /></span>
        </label>
        <label className="login-remember"><input type="checkbox" checked={rememberMe} onChange={(event) => setRememberMe(event.target.checked)} /><span>Recordarme en este dispositivo</span></label>
        <button className="login-submit" type="submit" disabled={loading}>
          {loading ? <><LoaderCircle className="loading-spinner" size={17} /> Validando acceso...</> : 'Ingresar'}
        </button>
      </form>

      <div className="login-credentials"><b>Acceso inicial</b><span>Usuario: <strong>jefe_admin</strong></span><span>Contraseña: <strong>Password123!</strong></span></div>
      <p className="login-help">Sin conexión con la API, estas credenciales habilitan una sesión de demostración local en este navegador.</p>
    </section>
    <footer className="login-footer">Sistema Integral de Gestión de Eventos y Personal</footer>
  </main>
}
