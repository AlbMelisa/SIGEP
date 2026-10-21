import { useEffect } from 'react'

export function LoginTransition({ type = 'login', onComplete }) {
  useEffect(() => {
    const timeout = window.setTimeout(onComplete, 3000)
    return () => window.clearTimeout(timeout)
  }, [onComplete])

  return <main className="login-transition-screen" aria-label="Ingresando al sistema">
    <img className="login-transition-logo" src="/sigep-logo-transparente.png" alt="SIGEP · Municipalidad de Simoca" />
    <span className="login-transition-message">{type === 'logout' ? 'Cerrando sesión' : 'Preparando tu espacio de trabajo'}</span>
  </main>
}
