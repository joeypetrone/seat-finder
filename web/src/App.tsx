import { useState, useEffect } from 'react'

type ApiStatus = 'checking' | 'healthy' | 'down'
const statusLabels: Record<ApiStatus, string> = {
  checking: 'Checking…',
  healthy: 'Healthy',
  down: 'Down',
}

function App() {
  const [status, setStatus] = useState<ApiStatus>('checking')
  useEffect(() => {
    async function checkHealth() {
      try {
        const response = await fetch('/health')
        const body = await response.text()
        if (response.ok && body === 'Healthy') {
          setStatus('healthy')
        } else {
          setStatus('down')
        }
      } catch {
        setStatus('down')
      }
    }
    checkHealth()
  }, [])

  return (
    <main>
      <h1>Seat finder</h1>
      <p>API: {statusLabels[status]}</p>
    </main>
  )
}

export default App
