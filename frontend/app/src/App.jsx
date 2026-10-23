import { useState, useEffect } from 'react'
import {
  Activity, ArrowLeft, Bell, CalendarDays, Check, ChevronDown, ChevronLeft, ChevronRight, CircleHelp,
  Clock3, LayoutDashboard, LoaderCircle, LogIn, LogOut, Menu, Plus, Search, SlidersHorizontal, Users, X,
} from 'lucide-react'
import './App.css'
import { useAuth } from './context/AuthContext.jsx'
import { LoginPage } from './components/LoginPage.jsx'
import { LoginModal } from './components/LoginModal.jsx'
import { LoginTransition } from './components/LoginTransition.jsx'
import { personalApi, asistenciaApi, sabadosApi, eventosApi } from './services/api.js'

const initialEmployees = [
  { id: 1, name: 'Lucía Gómez', dni: '32.456.781', phone: '', funcion: 'Servicios', regime: 'Incorporado', shift: 'Mañana', status: 'Activo' },
  { id: 2, name: 'Martín Díaz', dni: '30.127.903', phone: '', funcion: 'Tránsito', regime: 'Incorporado', shift: 'Siesta', status: 'Activo' },
  { id: 3, name: 'Carla Ruiz', dni: '28.440.215', phone: '', funcion: 'Administración', regime: 'Histórico', shift: 'Mañana', status: 'Activo' },
  { id: 4, name: 'Diego López', dni: '27.968.540', phone: '', funcion: 'Servicios', regime: 'Histórico', shift: 'Siesta', status: 'Activo' },
  { id: 5, name: 'Ana Medina', dni: '33.024.719', phone: '', funcion: 'Cultura', regime: 'Incorporado', shift: 'Noche', status: 'Activo' },
  { id: 6, name: 'José Pérez', dni: '29.813.004', phone: '', funcion: 'Servicios', regime: 'Histórico', shift: 'Noche', status: 'Activo' },
  { id: 7, name: 'Mónica Sosa', dni: '31.444.229', phone: '', funcion: 'Desarrollo Social', regime: 'Incorporado', shift: 'Mañana', status: 'Activo' },
  { id: 8, name: 'Raúl Fernández', dni: '26.880.331', phone: '', funcion: 'Tránsito', regime: 'Histórico', shift: 'Siesta', status: 'Activo' },
]

const initialAttendance = [
  { id: 1, entry: '06:02', exit: '12:00', status: 'Presente', tieneFrancoDisponible: false },
  { id: 2, entry: '12:04', exit: '18:00', status: 'Presente', tieneFrancoDisponible: false },
  { id: 3, entry: '07:00', exit: '13:00', status: 'Presente', tieneFrancoDisponible: false },
  { id: 4, entry: '', exit: '', status: 'Licencia', tieneFrancoDisponible: false },
  { id: 5, entry: '18:00', exit: '', status: 'Presente', tieneFrancoDisponible: false },
  { id: 6, entry: '', exit: '', status: 'Franco', tieneFrancoDisponible: true },
  { id: 7, entry: '06:00', exit: '', status: 'Presente', tieneFrancoDisponible: false },
  { id: 8, entry: '13:00', exit: '', status: 'Presente', tieneFrancoDisponible: false },
]

const initialEvents = [
  { id: 1, name: 'Feria municipal', date: '2026-10-03', time: '17:00', place: 'Plaza principal', people: 0, state: 'Borrador' },
  { id: 2, name: 'Acto institucional', date: '2026-10-08', time: '09:00', place: 'Casa de la Cultura', people: 0, state: 'Confirmado' },
  { id: 3, name: 'Jornada comunitaria', date: '2026-10-12', time: '08:30', place: 'Parque municipal', people: 0, state: 'Borrador' },
]

const navItems = [
  { id: 'inicio', label: 'Inicio', icon: LayoutDashboard },
  { id: 'personal', label: 'Personal', icon: Users },
  { id: 'asistencia', label: 'Asistencia', icon: Clock3 },
  { id: 'eventos', label: 'Eventos', icon: CalendarDays },
  { id: 'sabados', label: 'Planificación de sábados', icon: Activity },
]

const attendanceStatuses = ['Presente', 'Ausente', 'Franco', 'Nota médica', 'Licencia', 'Permiso especial']
const convocationAttendanceStatuses = ['Pendiente', 'Presente', 'Ausente', 'Justificado']
const demoDate = new Date().toISOString().split('T')[0]
const monthLabels = ['ENE', 'FEB', 'MAR', 'ABR', 'MAY', 'JUN', 'JUL', 'AGO', 'SEP', 'OCT', 'NOV', 'DIC']
const pageSize = 6

function formatDate(value) {
  if (!value) return ''
  const [year, month, day] = value.split('T')[0].split('-')
  return `${day}/${month}/${year}`
}

function dateHasPassed(value) {
  if (!value) return false
  const now = new Date()
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
  return value < today
}

function nextSaturdayISO() {
  const date = new Date()
  const daysUntilSaturday = (6 - date.getDay() + 7) % 7 || 7
  date.setDate(date.getDate() + daysUntilSaturday)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

function todayLabel() {
  return new Intl.DateTimeFormat('es-AR', { weekday: 'long', day: 'numeric', month: 'long' }).format(new Date())
}

function mapEmployeeFromBackend(emp) {
  return {
    id: emp.id,
    name: emp.nombreCompleto || `${emp.nombre} ${emp.apellido}`,
    dni: emp.documento,
    phone: emp.telefono || '',
    email: emp.email || '',
    direccion: emp.direccion || '',
    funcion: emp.funcionNombre || 'Servicios',
    funcionId: emp.funcionId,
    regime: emp.regimenNombre || 'Incorporado',
    regimenId: emp.regimenId,
    shift: emp.turnoHabitualNombre || 'Mañana',
    shiftId: emp.turnoHabitualId,
    status: emp.estado || 'Activo',
  }
}

function App() {
  const { user, isAuthenticated, logout, authTransition, completeAuthTransition } = useAuth()
  const [loginModalOpen, setLoginModalOpen] = useState(false)
  const [page, setPage] = useState('inicio')
  const [mobileNav, setMobileNav] = useState(false)
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false)
  const [notificationsOpen, setNotificationsOpen] = useState(false)
  
  // Datos reales / persistidos
  const [referencias, setReferencias] = useState({ funciones: [], regimenes: [], turnos: [] })
  const [employees, setEmployees] = useState(initialEmployees)
  const [attendance, setAttendance] = useState(initialAttendance)
  const [attendanceDate, setAttendanceDate] = useState(demoDate)
  const [events, setEvents] = useState(initialEvents)
  const [query, setQuery] = useState('')
  const [regimeFilter, setRegimeFilter] = useState('Todos los regímenes')
  const [attendanceTab, setAttendanceTab] = useState('registro')
  const [attendanceFilter, setAttendanceFilter] = useState('')
  const [notice, setNotice] = useState('')
  const [employeeModal, setEmployeeModal] = useState(false)
  const [editingEmployeeId, setEditingEmployeeId] = useState(null)
  const [eventModal, setEventModal] = useState(false)
  const [isSavingEvent, setIsSavingEvent] = useState(false)
  const [eventSaveError, setEventSaveError] = useState('')
  const [employeeForm, setEmployeeForm] = useState({ name: '', dni: '', phone: '', funcion: '', regime: 'Incorporado', shift: 'Mañana' })
  const [eventForm, setEventForm] = useState({ name: '', date: '', time: '', place: '' })
  const [saturdayGenerated, setSaturdayGenerated] = useState(false)
  const [saturdayLoading, setSaturdayLoading] = useState(false)
  const [saturdayPeople, setSaturdayPeople] = useState([])
  const [saturdayDate, setSaturdayDate] = useState(nextSaturdayISO)
  const [saturdayRegime, setSaturdayRegime] = useState('Incorporado')
  const [saturdayConvocations, setSaturdayConvocations] = useState({})
  const [currentSaturdayPlanId, setCurrentSaturdayPlanId] = useState(null)
  const [assignmentGenerated, setAssignmentGenerated] = useState(false)
  const [assignmentLoading, setAssignmentLoading] = useState(false)
  const [selectedEvent, setSelectedEvent] = useState(0)
  const [assignmentEditStart, setAssignmentEditStart] = useState(false)
  const [selectedPeople, setSelectedPeople] = useState([])
  const [assignmentShifts, setAssignmentShifts] = useState({})
  const [eventTurns, setEventTurns] = useState({})
  const [confirmedEventTurns, setConfirmedEventTurns] = useState({})
  const [eventConvocations, setEventConvocations] = useState({})
  const [plannedEventIndexes, setPlannedEventIndexes] = useState([])

  const notify = (message) => {
    setNotice(message)
    window.clearTimeout(window.noticeTimer)
    window.noticeTimer = window.setTimeout(() => setNotice(''), 3000)
  }

  const go = (id) => {
    setPage(id)
    setMobileNav(false)
  }

  // Cargar datos reales desde la API al autenticarse
  useEffect(() => {
    if (!isAuthenticated) return

    async function loadData() {
      try {
        const [refData, empData, evtData, sabData, attData] = await Promise.allSettled([
          personalApi.referencias(),
          personalApi.listar(),
          eventosApi.listarActivos(),
          sabadosApi.obtenerHistorial(10),
          asistenciaApi.obtenerPlanilla(demoDate),
        ])

        if (refData.status === 'fulfilled') {
          setReferencias(refData.value)
        }

        if (empData.status === 'fulfilled' && Array.isArray(empData.value)) {
          setEmployees(empData.value.map(mapEmployeeFromBackend))
        }

        if (evtData.status === 'fulfilled' && Array.isArray(evtData.value)) {
          if (evtData.value.length > 0) {
            setEvents(evtData.value.map(e => ({
              id: e.id,
              name: e.nombre,
              date: e.fechaInicio,
              time: '09:00',
              place: e.lugar || 'Simoca',
              people: e.totalPersonalAsignado || 0,
              state: e.estado === 'Planificado' ? 'Borrador' : 'Confirmado',
            })))
          }
        }

        if (attData.status === 'fulfilled' && Array.isArray(attData.value)) {
          setAttendance(attData.value.map(item => ({
            id: item.empleadoId,
            asistenciaId: item.asistenciaId,
            entry: item.horaIngreso || '',
            exit: item.horaSalida || '',
            status: item.estado === 'NotaMedica' ? 'Nota médica' : item.estado === 'PermisoEspecial' ? 'Permiso especial' : item.estado,
            observacion: item.observacion || '',
            tieneFrancoDisponible: Boolean(item.tieneFrancoDisponible),
            detalleFranco: item.detalleFranco,
          })))
        }

        if (sabData.status === 'fulfilled' && Array.isArray(sabData.value)) {
          const mapConvocations = {}
          sabData.value.forEach(p => {
            const key = `${p.fechaSabado}:Incorporado`
            mapConvocations[key] = {
              id: p.id,
              date: p.fechaSabado,
              regime: 'Incorporado',
              confirmedAt: p.fechaConfirmacion || p.fechaCreacion,
              totalAsignados: p.totalAsignados,
              people: [],
            }
          })
          setSaturdayConvocations(mapConvocations)
        }
      } catch (err) {
        console.error('Error cargando datos iniciales:', err)
      }
    }

    loadData()
  }, [isAuthenticated])

  // Cargar planilla de asistencia al cambiar la fecha seleccionada
  const fetchAttendanceSheet = async (date) => {
    try {
      setAttendanceDate(date)
      const data = await asistenciaApi.obtenerPlanilla(date)
      if (Array.isArray(data)) {
        setAttendance(data.map(item => ({
          id: item.empleadoId,
          asistenciaId: item.asistenciaId,
          entry: item.horaIngreso || '',
          exit: item.horaSalida || '',
          status: item.estado === 'NotaMedica' ? 'Nota médica' : item.estado === 'PermisoEspecial' ? 'Permiso especial' : item.estado,
          observacion: item.observacion || '',
          tieneFrancoDisponible: Boolean(item.tieneFrancoDisponible),
          detalleFranco: item.detalleFranco,
        })))
      }
    } catch (err) {
      notify(err.message || 'Error al obtener planilla diaria')
    }
  }

  const filteredEmployees = employees.filter((employee) => {
    const searchMatches = `${employee.name} ${employee.dni}`.toLowerCase().includes(query.trim().toLowerCase())
    const regimeMatches = regimeFilter === 'Todos los regímenes' || employee.regime === regimeFilter
    return searchMatches && regimeMatches
  })
  const activeEmployees = employees.filter((employee) => employee.status === 'Activo')

  const updateAttendance = (id, field, value) => {
    setAttendance((current) => current.map((row) => row.id === id ? { ...row, [field]: value } : row))
  }

  const openNewEmployee = () => {
    setEditingEmployeeId(null)
    setEmployeeForm({ name: '', dni: '', phone: '', funcion: '', regime: 'Incorporado', shift: 'Mañana' })
    setEmployeeModal(true)
  }

  const editEmployee = (employee) => {
    setEditingEmployeeId(employee.id)
    setEmployeeForm({ name: employee.name, dni: employee.dni, phone: employee.phone ?? '', funcion: employee.funcion, regime: employee.regime, shift: employee.shift })
    setEmployeeModal(true)
  }

  const toggleEmployeeStatus = async (id) => {
    const employee = employees.find((item) => item.id === id)
    if (!employee) return
    const nuevoEstado = employee.status === 'Activo' ? 'Inactivo' : 'Activo'
    
    try {
      await personalApi.cambiarEstado(id, nuevoEstado)
      setEmployees((current) => current.map((emp) => emp.id === id ? { ...emp, status: nuevoEstado } : emp))
      notify(`Estado de ${employee.name} actualizado a ${nuevoEstado}`)
    } catch (err) {
      notify(err.message || 'Error al cambiar estado')
    }
  }

  const closeEmployeeModal = () => {
    setEmployeeModal(false)
    setEditingEmployeeId(null)
  }

  const saveEmployee = async (event) => {
    event.preventDefault()
    try {
      const parts = employeeForm.name.trim().split(' ')
      const nombre = parts[0] || 'Empleado'
      const apellido = parts.slice(1).join(' ') || 'Simoca'
      
      const funcObj = referencias.funciones.find(f => f.nombre.toLowerCase() === employeeForm.funcion.toLowerCase()) || referencias.funciones[0]
      const regObj = referencias.regimenes.find(r => r.nombre.toLowerCase() === employeeForm.regime.toLowerCase()) || referencias.regimenes[0]
      const turnoObj = referencias.turnos.find(t => t.regimenId === (regObj?.id || 1) && t.nombre.toLowerCase().includes(employeeForm.shift.toLowerCase())) || referencias.turnos.find(t => t.regimenId === (regObj?.id || 1))

      if (editingEmployeeId === null) {
        const payload = {
          nombre,
          apellido,
          documento: employeeForm.dni.replace(/\D/g, ''),
          funcionId: funcObj ? funcObj.id : 1,
          regimenId: regObj ? regObj.id : 1,
          turnoHabitualId: turnoObj ? turnoObj.id : 1,
          telefono: employeeForm.phone,
          email: `${nombre.toLowerCase()}.${apellido.toLowerCase()}@simoca.gob.ar`,
          direccion: 'Simoca, Tucumán',
        }
        const created = await personalApi.crear(payload)
        setEmployees((current) => [...current, mapEmployeeFromBackend(created)])
        notify('Empleado registrado exitosamente')
      } else {
        const payload = {
          nombre,
          apellido,
          documento: employeeForm.dni.replace(/\D/g, ''),
          funcionId: funcObj ? funcObj.id : 1,
          regimenId: regObj ? regObj.id : 1,
          turnoHabitualId: turnoObj ? turnoObj.id : 1,
          telefono: employeeForm.phone,
          email: employeeForm.email || `${nombre.toLowerCase()}@simoca.gob.ar`,
          direccion: employeeForm.direccion || 'Simoca, Tucumán',
        }
        const updated = await personalApi.actualizar(editingEmployeeId, payload)
        setEmployees((current) => current.map((emp) => emp.id === editingEmployeeId ? mapEmployeeFromBackend(updated) : emp))
        notify('Legajo del empleado actualizado')
      }
      closeEmployeeModal()
    } catch (err) {
      notify(err.message || 'Error al guardar empleado')
    }
  }

  const saveAttendanceBatch = async () => {
    try {
      const items = attendance.map(row => {
        const emp = employees.find(e => e.id === row.id)
        let estadoNormalizado = row.status
        if (estadoNormalizado === 'Nota médica') estadoNormalizado = 'NotaMedica'
        if (estadoNormalizado === 'Permiso especial') estadoNormalizado = 'PermisoEspecial'

        return {
          empleadoId: row.id,
          turnoId: emp?.shiftId || 1,
          horaIngreso: row.status === 'Presente' && row.entry ? row.entry : null,
          horaSalida: row.status === 'Presente' && row.exit ? row.exit : null,
          estado: estadoNormalizado,
          observacion: row.observacion || null,
        }
      })

      await asistenciaApi.guardarPlanillaLote({
        fecha: attendanceDate,
        tipoJornada: 'Ordinaria',
        items,
      })
      notify('Asistencia guardada exitosamente')
      fetchAttendanceSheet(attendanceDate)
    } catch (err) {
      notify(err.message || 'Error al guardar asistencia')
    }
  }

  const saveEvent = async (event) => {
    event.preventDefault()
    if (!eventForm.name.trim()) {
      setEventSaveError('El nombre del evento es obligatorio.')
      return
    }
    if (!eventForm.date) {
      setEventSaveError('La fecha del evento es obligatoria.')
      return
    }
    setIsSavingEvent(true)
    setEventSaveError('')
    try {
      const payload = {
        nombre: eventForm.name.trim(),
        fechaInicio: eventForm.date,
        fechaFin: eventForm.date,
        lugar: eventForm.place?.trim() || 'Simoca, Tucumán',
        turnos: [] // No preasignamos ningún turno ni cantidad forzada
      }
      const nuevoEvento = await eventosApi.crear(payload)
      setEvents((current) => [...current, {
        id: nuevoEvento.id,
        name: nuevoEvento.nombre,
        date: nuevoEvento.fechaInicio,
        time: eventForm.time || '09:00',
        place: nuevoEvento.lugar || 'Simoca, Tucumán',
        people: 0,
        state: 'Borrador',
      }])
      setEventModal(false)
      setEventForm({ name: '', date: '', time: '', place: '' })
      notify('Evento creado exitosamente')
    } catch (err) {
      const msg = err.message || 'Error al crear evento'
      setEventSaveError(msg)
      notify(msg)
    } finally {
      setIsSavingEvent(false)
    }
  }

  const generateSaturday = async (regime = saturdayRegime) => {
    if (saturdayLoading) return
    setSaturdayLoading(true)
    try {
      const plan = await sabadosApi.generarPropuesta(saturdayDate, 'Generado desde el frontend SIGEP')
      setCurrentSaturdayPlanId(plan.id)
      
      const mappedPeople = plan.detalles.map(d => ({
        id: d.idEmpleado,
        name: d.nombreCompleto,
        funcion: d.funcion,
        regime: 'Incorporado',
        saturdayShift: d.nombreTurno,
        works: true,
        esManual: d.esModificacionManual,
      }))

      setSaturdayPeople(mappedPeople)
      setSaturdayGenerated(true)
      notify(`Propuesta para el sábado ${formatDate(saturdayDate)} generada con el 50% de rotación`)
    } catch (err) {
      // Si ya existe borrador, intentar consultarlo
      try {
        const existente = await sabadosApi.obtenerPorFecha(saturdayDate)
        if (existente) {
          setCurrentSaturdayPlanId(existente.id)
          const mappedPeople = existente.detalles.map(d => ({
            id: d.idEmpleado,
            name: d.nombreCompleto,
            funcion: d.funcion,
            regime: 'Incorporado',
            saturdayShift: d.nombreTurno,
            works: true,
            esManual: d.esModificacionManual,
          }))
          setSaturdayPeople(mappedPeople)
          setSaturdayGenerated(true)
          notify(`Se cargó la planificación existente para el sábado ${formatDate(saturdayDate)}`)
        } else {
          notify(err.message || 'Error al generar sábado')
        }
      } catch {
        notify(err.message || 'Error al generar propuesta de sábados')
      }
    } finally {
      setSaturdayLoading(false)
    }
  }

  const confirmSaturdayConvocation = async () => {
    if (!currentSaturdayPlanId) {
      notify('No hay un ID de planificación activa para confirmar.')
      return
    }
    try {
      const confirmado = await sabadosApi.confirmar(currentSaturdayPlanId)
      const key = `${saturdayDate}:Incorporado`
      setSaturdayConvocations((current) => ({
        ...current,
        [key]: {
          id: confirmado.id,
          date: saturdayDate,
          regime: 'Incorporado',
          people: saturdayPeople.map((person) => ({ ...person })),
          confirmedAt: new Date().toISOString(),
        },
      }))
      notify('Convocatoria de sábado confirmada exitosamente')
    } catch (err) {
      notify(err.message || 'Error al confirmar sábado')
    }
  }

  const toggleSaturdayPerson = (id) => {
    setSaturdayPeople((current) => current.map((person) => person.id === id ? { ...person, works: person.works === false } : person))
  }

  const changeSaturdayShift = (id, shift) => {
    setSaturdayPeople((current) => current.map((person) => person.id === id ? { ...person, saturdayShift: shift } : person))
  }

  const generateAssignment = async () => {
    if (assignmentLoading) return
    setAssignmentLoading(true)
    try {
      const curEvent = events[selectedEvent]
      if (curEvent?.id) {
        const fullEvent = await eventosApi.obtenerPorId(curEvent.id)
        if (fullEvent.turnos && fullEvent.turnos.length > 0) {
          const scoring = await eventosApi.previsualizarScoring(fullEvent.turnos[0].id)
          const selected = scoring.asignacionesPropuestas.map(p => p.idEmpleado)
          setSelectedPeople(selected)
          setAssignmentGenerated(true)
          notify(`Scoring calculado para ${scoring.asignacionesPropuestas.length} agentes. Excluidos por 48hs: ${scoring.agentesExcluidosPorDescanso.length}`)
          setAssignmentLoading(false)
          return
        }
      }

      // Fallback si no tiene turnos cargados
      const configuredTurns = eventTurns[selectedEvent] ?? []
      const totalRequested = configuredTurns.reduce((sum, t) => sum + Number(t.people), 0) || (events[selectedEvent]?.people ?? 0) || 4
      const count = Math.min(totalRequested, activeEmployees.length)
      const selected = activeEmployees.slice(0, count).map((employee) => employee.id)
      setSelectedPeople(selected)
      setAssignmentGenerated(true)
      notify(`Propuesta preliminar calculada para ${count} agentes`)
    } catch (err) {
      notify(err.message || 'Error calculando scoring')
    } finally {
      setAssignmentLoading(false)
    }
  }

  const selectAssignmentEvent = (index) => {
    setSelectedEvent(index)
    const plan = eventConvocations[index]
    setSelectedPeople(plan?.people.map((person) => person.id) ?? [])
    setAssignmentShifts(plan?.assignments ?? {})
    setAssignmentGenerated(Boolean(plan))
  }

  const toggleAssigned = (id) => {
    setSelectedPeople((current) => current.includes(id) ? current.filter((selected) => selected !== id) : [...current, id])
  }

  const changeEventTurns = (index, turns) => {
    setEventTurns((current) => ({ ...current, [index]: turns }))
    setConfirmedEventTurns((current) => ({ ...current, [index]: false }))
    setEventConvocations((current) => {
      const updated = { ...current }
      delete updated[index]
      return updated
    })
    setPlannedEventIndexes((current) => current.filter((item) => item !== index))
    setAssignmentGenerated(false)
    setSelectedPeople([])
    setAssignmentShifts({})
  }

  const confirmEventConvocation = (turns) => {
    const selected = employees.filter((employee) => selectedPeople.includes(employee.id))
    const previousAttendance = new Map((eventConvocations[selectedEvent]?.people ?? []).map((person) => [person.id, person.attendance ?? 'Pendiente']))
    setEventConvocations((current) => ({
      ...current,
      [selectedEvent]: {
        people: selected.map((employee) => ({ ...employee, attendance: previousAttendance.get(employee.id) ?? 'Pendiente' })),
        assignments: { ...assignmentShifts },
        turns: turns.map((turn) => ({ ...turn })),
        confirmedAt: new Date().toISOString(),
      },
    }))
    setPlannedEventIndexes((current) => current.includes(selectedEvent) ? current : [...current, selectedEvent])
    notify('Convocatoria del evento confirmada')
  }

  const updateEventAttendance = (eventIndex, employeeId, status) => {
    setEventConvocations((current) => {
      const plan = current[eventIndex]
      if (!plan) return current
      return { ...current, [eventIndex]: { ...plan, people: plan.people.map((person) => person.id === employeeId ? { ...person, attendance: status } : person) } }
    })
  }

  const changeSaturdaySelection = async (date, regime) => {
    setSaturdayDate(date)
    setSaturdayRegime(regime)
    setSaturdayLoading(false)
    
    try {
      const planExistente = await sabadosApi.obtenerPorFecha(date)
      if (planExistente) {
        setCurrentSaturdayPlanId(planExistente.id)
        setSaturdayGenerated(true)
        setSaturdayPeople(planExistente.detalles.map(d => ({
          id: d.idEmpleado,
          name: d.nombreCompleto,
          funcion: d.funcion,
          regime: 'Incorporado',
          saturdayShift: d.nombreTurno,
          works: true,
        })))
        return
      }
    } catch {
      // Ignorar si no existe
    }

    const plan = saturdayConvocations[`${date}:${regime}`]
    setSaturdayGenerated(Boolean(plan))
    setSaturdayPeople(plan?.people ?? [])
  }

  const setEventAssignmentShift = (employeeId, turnId) => {
    setAssignmentShifts((current) => ({ ...current, [employeeId]: turnId }))
  }

  const notifications = [
    ...(!saturdayGenerated && !dateHasPassed(saturdayDate) ? [{ id: 'saturday', title: 'Planificación de sábados pendiente', detail: 'Generá la planificación para organizar los turnos.', page: 'sabados' }] : []),
    ...events.filter((event, index) => event.state === 'Borrador' && !plannedEventIndexes.includes(index) && !dateHasPassed(event.date)).map((event) => ({ id: `event-${event.name}`, title: 'Evento pendiente de planificación', detail: event.name, page: 'eventos' })),
  ]

  const pageTitle = page === 'asignacion' ? 'Planificación de eventos' : navItems.find((item) => item.id === page)?.label ?? 'Inicio'

  if (authTransition) return <LoginTransition type={authTransition} onComplete={completeAuthTransition} />
  if (!isAuthenticated) return <LoginPage />

  return (
    <div className={`app-shell ${sidebarCollapsed ? 'sidebar-collapsed' : ''}`}>
      <aside className={`sidebar ${mobileNav ? 'sidebar-open' : ''}`}>
        <a className="brand" href="#inicio" onClick={() => go('inicio')} aria-label="SIGEP, inicio">
          <img src="/sigep-logo-transparente.png" alt="SIGEP · Sistema Integral de Gestión de Eventos y Personal · Municipalidad de Simoca" />
        </a>
        <div className="workspace-label">GESTIÓN MUNICIPAL</div>
        <nav className="nav-list" aria-label="Navegación principal">
          {navItems.map(({ id, label, icon: Icon }) => (
            <button className={`nav-item ${page === id || (id === 'eventos' && page === 'asignacion') ? 'nav-item-active' : ''}`} key={id} onClick={() => go(id)}>
              <Icon size={18} strokeWidth={1.8} /><span>{label}</span>
              {id === 'asistencia' && <span className="nav-count">{attendance.length}</span>}
            </button>
          ))}
        </nav>
        <button className="sidebar-toggle" aria-label={sidebarCollapsed ? 'Expandir menú' : 'Minimizar menú'} title={sidebarCollapsed ? 'Expandir menú' : 'Minimizar menú'} onClick={() => setSidebarCollapsed((collapsed) => !collapsed)}>
          {sidebarCollapsed ? <ChevronRight size={18} /> : <ChevronLeft size={18} />}
        </button>
        <div className="sidebar-bottom">
          <div className="help-link"><CircleHelp size={17} /><span>Ayuda y soporte</span></div>
          {isAuthenticated && user ? (
            <div className="profile-row" style={{ cursor: 'pointer' }} onClick={() => logout()} title="Haz clic para cerrar sesión">
              <div className="avatar">
                {user.username.substring(0, 2).toUpperCase()}
              </div>
              <div className="profile-copy">
                <b>{user.username}</b>
                <span>{user.rol === 'Jefe' ? 'Jefe de personal' : 'Encargado de asistencia'}</span>
              </div>
              <LogOut size={15} color="#9eb1c3" />
            </div>
          ) : (
            <div className="profile-row" style={{ cursor: 'pointer' }} onClick={() => setLoginModalOpen(true)}>
              <div className="avatar" style={{ background: '#dce8d8', color: '#17392f' }}>
                <LogIn size={15} />
              </div>
              <div className="profile-copy">
                <b>Acceso operadores</b>
                <span>Iniciar sesión</span>
              </div>
              <ChevronDown size={15} />
            </div>
          )}
        </div>
      </aside>

      {mobileNav && <button className="mobile-scrim" aria-label="Cerrar menú" onClick={() => setMobileNav(false)} />}

      <main className="main-area">
        <header className="topbar">
          <button className="icon-button mobile-menu" aria-label="Abrir menú" onClick={() => setMobileNav(true)}><Menu size={20} /></button>
          <div className="breadcrumb"><span>SIGEP</span><ChevronRight size={14} /><b>{pageTitle}</b></div>
          <div className="notification-area">
            <button className={`notification-button ${notificationsOpen ? 'notification-button-active' : ''}`} aria-label={`Notificaciones${notifications.length ? `, ${notifications.length} pendientes` : ''}`} aria-expanded={notificationsOpen} onClick={() => setNotificationsOpen((open) => !open)}>
              <Bell size={19} />
              {notifications.length > 0 && <span className="notification-count">{notifications.length}</span>}
            </button>
            {notificationsOpen && <div className="notification-popover"><div className="notification-heading"><b>Notificaciones</b><span>{notifications.length} pendientes</span></div>{notifications.length ? notifications.map((item) => <button className="notification-item" key={item.id} onClick={() => { go(item.page); setNotificationsOpen(false) }}><span className="notification-mark" /><span><b>{item.title}</b><small>{item.detail}</small></span><ChevronRight size={15} /></button>) : <p className="notification-empty">No hay planificaciones pendientes.</p>}</div>}
          </div>
        </header>

        <div className="page-content">
          {page === 'inicio' && <Dashboard employees={activeEmployees} attendance={attendance} events={events} eventConvocations={eventConvocations} go={go} />}
          {page === 'personal' && <PersonalPage employees={filteredEmployees} query={query} setQuery={setQuery} regimeFilter={regimeFilter} setRegimeFilter={setRegimeFilter} onAdd={openNewEmployee} onEdit={editEmployee} onToggleStatus={toggleEmployeeStatus} />}
          {page === 'asistencia' && <AttendancePage employees={activeEmployees} attendance={attendance} attendanceDate={attendanceDate} onDateChange={fetchAttendanceSheet} updateAttendance={updateAttendance} tab={attendanceTab} setTab={setAttendanceTab} shiftFilter={attendanceFilter} setShiftFilter={setAttendanceFilter} onSave={saveAttendanceBatch} />}
          {page === 'eventos' && <EventsPage events={events} eventConvocations={eventConvocations} plannedEventIndexes={plannedEventIndexes} onAdd={() => setEventModal(true)} onPlanEvent={(index, edit) => { selectAssignmentEvent(index); setAssignmentEditStart(Boolean(edit)); go('asignacion') }} />}
          {page === 'sabados' && <SaturdayPage employees={activeEmployees} generated={saturdayGenerated} loading={saturdayLoading} people={saturdayPeople} workDate={saturdayDate} regime={saturdayRegime} convocation={saturdayConvocations[`${saturdayDate}:${saturdayRegime}`] ?? null} convocations={saturdayConvocations} onSelectionChange={changeSaturdaySelection} onGenerate={generateSaturday} onToggle={toggleSaturdayPerson} onShift={changeSaturdayShift} onRestore={() => changeSaturdaySelection(saturdayDate, saturdayRegime)} onConfirm={confirmSaturdayConvocation} />}
          {page === 'asignacion' && <AssignmentPage events={events} employees={activeEmployees} selectedEvent={selectedEvent} setSelectedEvent={selectAssignmentEvent} startEditing={assignmentEditStart} generated={assignmentGenerated} loading={assignmentLoading} selectedPeople={selectedPeople} assignmentShifts={assignmentShifts} turns={eventTurns[selectedEvent] ?? []} turnsConfirmed={confirmedEventTurns[selectedEvent] === true} convocation={eventConvocations[selectedEvent] ?? null} onGenerate={generateAssignment} onToggle={toggleAssigned} onShiftChange={setEventAssignmentShift} onTurnChange={changeEventTurns} onTurnsConfirm={(index) => setConfirmedEventTurns((current) => ({ ...current, [index]: true }))} onAttendanceChange={updateEventAttendance} onConfirm={confirmEventConvocation} go={go} />}
        </div>
      </main>

      {employeeModal && <Modal title={editingEmployeeId === null ? 'Nuevo empleado municipal' : 'Editar empleado'} onClose={closeEmployeeModal}><form className="form-grid" onSubmit={saveEmployee}>
        <label>Nombre y apellido<input required value={employeeForm.name} onChange={(event) => setEmployeeForm({ ...employeeForm, name: event.target.value })} placeholder="Ej. María Gómez" /></label>
        <label>DNI<input required value={employeeForm.dni} onChange={(event) => setEmployeeForm({ ...employeeForm, dni: event.target.value })} placeholder="00.000.000" /></label>
        <label>Celular<input required type="tel" value={employeeForm.phone} onChange={(event) => setEmployeeForm({ ...employeeForm, phone: event.target.value })} placeholder="Ej. 381 555 0123" /></label>
        <label>Función<select value={employeeForm.funcion} onChange={(event) => setEmployeeForm({ ...employeeForm, funcion: event.target.value })}>{(referencias.funciones.length ? referencias.funciones.map(f => f.nombre) : ['Servicios', 'Tránsito', 'Administración', 'Cultura', 'Desarrollo Social']).map(f => <option key={f}>{f}</option>)}</select></label>
        <label>Régimen<select value={employeeForm.regime} onChange={(event) => setEmployeeForm({ ...employeeForm, regime: event.target.value })}><option>Incorporado</option><option>Histórico</option></select></label>
        <label>Turno habitual<select value={employeeForm.shift} onChange={(event) => setEmployeeForm({ ...employeeForm, shift: event.target.value })}><option>Mañana</option><option>Siesta</option><option>Tarde</option><option>Noche</option></select></label>
        <div className="modal-actions"><button type="button" className="button button-quiet" onClick={closeEmployeeModal}>Cancelar</button><button className="button button-primary">{editingEmployeeId === null ? 'Guardar' : 'Guardar cambios'}</button></div>
      </form></Modal>}

      {eventModal && <Modal title="Crear evento municipal" onClose={() => { if (!isSavingEvent) { setEventModal(false); setEventSaveError('') } }}><form className="form-grid" onSubmit={saveEvent}>
        <label className="form-wide">Nombre del evento<input required disabled={isSavingEvent} value={eventForm.name} onChange={(event) => setEventForm({ ...eventForm, name: event.target.value })} placeholder="Ej. Fiesta Nacional del Sulky" /></label>
        <label>Fecha<input required disabled={isSavingEvent} type="date" value={eventForm.date} onChange={(event) => setEventForm({ ...eventForm, date: event.target.value })} /></label>
        <label>Hora<input disabled={isSavingEvent} type="time" value={eventForm.time} onChange={(event) => setEventForm({ ...eventForm, time: event.target.value })} /></label>
        <label className="form-wide">Lugar<input disabled={isSavingEvent} value={eventForm.place} onChange={(event) => setEventForm({ ...eventForm, place: event.target.value })} placeholder="Plaza Bartolomé Mitre / Predio Ferial" /></label>
        {eventSaveError && <p className="form-wide event-turn-error" role="alert">{eventSaveError}</p>}
        <div className="modal-actions">
          <button type="button" className="button button-quiet" disabled={isSavingEvent} onClick={() => { setEventModal(false); setEventSaveError('') }}>Cancelar</button>
          <button type="submit" className="button button-primary" disabled={isSavingEvent}>
            {isSavingEvent ? 'Guardando...' : 'Guardar'}
          </button>
        </div>
      </form></Modal>}

      {notice && <div className="toast" role="status"><Check size={16} />{notice}</div>}
    </div>
  )
}

function PageHeading({ eyebrow, title, description, action }) {
  return <div className="page-heading"><div><div className="eyebrow">{eyebrow}</div><h1>{title}</h1><p>{description}</p></div>{action}</div>
}

function Dashboard({ employees, attendance, events, eventConvocations, go }) {
  const activeEmployeeIds = new Set(employees.map((employee) => employee.id))
  const presentCount = attendance.filter((row) => row.status === 'Presente' && activeEmployeeIds.has(row.id)).length
  const regimeCounts = ['Incorporado', 'Histórico'].map((regime) => ({ regime, count: employees.filter((employee) => employee.regime === regime).length }))
  const eventQueue = events
    .map((event, index) => ({ event, index }))
    .sort((a, b) => Number(Boolean(eventConvocations[b.index])) - Number(Boolean(eventConvocations[a.index])) || a.event.date.localeCompare(b.event.date))
  return <>
    <PageHeading eyebrow={todayLabel()} title="Panel general" description="Una mirada clara al estado operativo del personal municipal." action={<button className="button button-outline" onClick={() => go('asistencia')}><Clock3 size={16} /> Ir a asistencia</button>} />
    <section className="metric-grid" aria-label="Indicadores principales">
      <Metric label="Personal activo" value={employees.length} caption="En ambos regímenes" icon={Users} tone="green" />
      <Metric label="Presentes hoy" value={presentCount} caption={`de ${employees.length} personas`} icon={Check} tone="blue" />
      <Metric label="Próximos eventos" value={events.length} caption="En agenda municipal" icon={CalendarDays} tone="coral" />
      <Metric label="Pendientes" value={events.filter((event) => event.state === 'Borrador').length} caption="Requieren confirmación" icon={Clock3} tone="gold" />
    </section>
    <section className="dashboard-grid">
      <div className="panel distribution-panel">
        <div className="panel-heading"><div><span className="eyebrow">COMPOSICIÓN</span><h2>Personal por régimen</h2></div><button className="text-button" onClick={() => go('personal')}>Ver personal <ChevronRight size={15} /></button></div>
        <div className="distribution-total"><b>{employees.length}</b><span>personas registradas</span></div>
        {regimeCounts.map(({ regime, count }, index) => <div className="regime-row" key={regime}><div className="regime-meta"><span>{regime}</span><b>{count} <small>({employees.length ? Math.round(count / employees.length * 100) : 0}%)</small></b></div><div className="progress-track"><span className={index === 0 ? 'progress-green' : 'progress-coral'} style={{ width: `${employees.length ? count / employees.length * 100 : 0}%` }} /></div></div>)}
        <div className="panel-footnote">La modalidad define el turno habitual asignado.</div>
      </div>
      <div className="panel events-panel">
        <div className="panel-heading"><div><span className="eyebrow">AGENDA</span><h2>Próximos eventos</h2></div><button className="icon-button" aria-label="Ver eventos" onClick={() => go('eventos')}><ChevronRight size={18} /></button></div>
        <div className="event-list">{eventQueue.slice(0, 3).map(({ event }) => <EventRow event={event} key={event.name} />)}</div>
        <button className="panel-link" onClick={() => go('eventos')}>Ver agenda completa <ChevronRight size={15} /></button>
      </div>
    </section>
    <div className="workflow-strip"><div className="workflow-icon"><Activity size={19} /></div><div><b>La planificación comienza con la asistencia.</b><span>Registrá la jornada y mantené el historial al día para preparar propuestas más claras.</span></div><button className="button button-quiet" onClick={() => go('asistencia')}>Abrir registro <ChevronRight size={15} /></button></div>
  </>
}

function Metric({ label, value, caption, icon: Icon, tone }) {
  return <article className="metric-card"><div className={`metric-icon ${tone}`}><Icon size={18} /></div><span className="metric-label">{label}</span><div className="metric-value">{value}</div><span className="metric-caption">{caption}</span></article>
}

function EventRow({ event }) {
  const [, month, day] = event.date.split('-')
  return <div className="event-row"><div className="event-date"><b>{day}</b><span>{monthLabels[Number(month) - 1]}</span></div><div className="event-copy"><b>{event.name}</b><span>{event.time} · {event.place}</span></div><StatusBadge value={event.state} /></div>
}

function StatusBadge({ value }) {
  const className = value === 'Confirmado' || value === 'Presente' || value === 'Activo' ? 'status-good' : value === 'Borrador' || value === 'Franco' || value === 'Licencia' ? 'status-wait' : value === 'Ausente' ? 'status-bad' : value === 'Inactivo' ? 'status-inactive' : 'status-neutral'
  return <span className={`status-badge ${className}`}><i />{value}</span>
}

function PersonalPage({ employees, query, setQuery, regimeFilter, setRegimeFilter, onAdd, onEdit, onToggleStatus }) {
  const [currentPage, setCurrentPage] = useState(1)
  const pageCount = Math.max(1, Math.ceil(employees.length / pageSize))
  const visibleEmployees = employees.slice((currentPage - 1) * pageSize, currentPage * pageSize)
  return <>
    <PageHeading eyebrow="DIRECTORIO MUNICIPAL" title="Personal" description="Consultá y administrá los datos del equipo y sus turnos habituales." action={<button className="button button-primary" onClick={onAdd}><Plus size={17} /> Nuevo empleado</button>} />
    <div className="table-panel"><div className="table-toolbar"><label className="field-search"><Search size={16} /><input aria-label="Buscar personal por nombre o DNI" value={query} onChange={(event) => { setCurrentPage(1); setQuery(event.target.value) }} placeholder="Buscar por nombre o DNI" /></label><label className="select-filter"><SlidersHorizontal size={15} /><select aria-label="Filtrar por régimen" value={regimeFilter} onChange={(event) => { setCurrentPage(1); setRegimeFilter(event.target.value) }}><option>Todos los regímenes</option><option>Incorporado</option><option>Histórico</option></select></label><span className="results-count">{employees.length} registros</span></div>
      <div className="table-scroll"><table><thead><tr><th>EMPLEADO</th><th>DNI</th><th>CELULAR</th><th>FUNCIÓN</th><th>RÉGIMEN</th><th>TURNO HABITUAL</th><th>ESTADO</th><th>ACCIONES</th></tr></thead><tbody>{visibleEmployees.map((employee) => <tr key={employee.id}><td><div className="employee-cell"><div className="avatar avatar-small">{employee.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><b>{employee.name}</b></div></td><td>{employee.dni}</td><td>{employee.phone || '—'}</td><td>{employee.funcion}</td><td><span className={`regime-chip regime-${employee.regime.toLowerCase()}`}>{employee.regime}</span></td><td><span className="shift-text"><Clock3 size={14} />{employee.shift}</span></td><td><StatusBadge value={employee.status} /></td><td><div className="employee-actions"><button className="button button-small button-outline" onClick={() => onEdit(employee)}>Editar</button><button className="button button-small button-quiet" onClick={() => onToggleStatus(employee.id)}>{employee.status === 'Activo' ? 'Dar de baja' : 'Reactivar'}</button></div></td></tr>)}</tbody></table>{employees.length === 0 && <div className="empty-state">No encontramos personas con esos filtros.</div>}</div>
      <Pagination currentPage={currentPage} pageCount={pageCount} total={employees.length} onChange={setCurrentPage} />
      <div className="table-footer"><span>Directorio activo de personal municipal.</span><span>Mostrando {employees.length} personas</span></div>
    </div>
  </>
}

function AttendancePage({ employees, attendance, attendanceDate, onDateChange, updateAttendance, tab, setTab, shiftFilter, setShiftFilter, onSave }) {
  const [search, setSearch] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const rows = employees.map((employee) => ({ employee, record: attendance.find((row) => row.id === employee.id) ?? { entry: '', exit: '', status: 'Ausente', tieneFrancoDisponible: false } }))
    .filter(({ employee }) => (!shiftFilter || employee.shift === shiftFilter)
      && `${employee.name} ${employee.dni}`.toLowerCase().includes(search.trim().toLowerCase()))
  const visibleRows = rows.slice((currentPage - 1) * pageSize, currentPage * pageSize)
  const shifts = ['', 'Mañana', 'Siesta', 'Tarde', 'Noche']
  const shiftLabels = { '': 'Todos', Mañana: 'Mañana', Siesta: 'Siesta', Tarde: 'Tarde', Noche: 'Noche' }
  const shiftCounts = employees.reduce((counts, employee) => ({ ...counts, [employee.shift]: (counts[employee.shift] ?? 0) + 1 }), {})
  
  return <>
    <PageHeading eyebrow="REGISTRO DIARIO Y FRANCOS COMPENSATORIOS" title="Asistencia" description="Cargá la jornada del equipo con validación automática de francos por sábados trabajados." action={tab === 'registro' ? <button className="button button-primary" onClick={onSave}><Check size={16} /> Guardar</button> : null} />
    <div className="tabs"><button className={tab === 'registro' ? 'tab-active' : ''} onClick={() => setTab('registro')}>Registro del día</button><button className={tab === 'historial' ? 'tab-active' : ''} onClick={() => setTab('historial')}>Historial</button></div>
    {tab === 'registro' ? <div className="table-panel">
      <div className="table-toolbar"><label className="field-search"><Search size={16} /><input aria-label="Buscar para registrar asistencia" value={search} onChange={(event) => { setSearch(event.target.value); setCurrentPage(1) }} placeholder="Buscar por nombre o DNI" /></label><label className="date-field"><CalendarDays size={16} /><input type="date" value={attendanceDate} onChange={(e) => onDateChange(e.target.value)} /></label><span className="results-count">{rows.filter(({ record }) => record.status === 'Presente').length} presentes</span></div>
      <div className="shift-tabs" role="group" aria-label="Filtrar asistencia por turno">{shifts.map((shift) => <button key={shift || 'todos'} className={shiftFilter === shift ? 'shift-tab-active' : ''} aria-pressed={shiftFilter === shift} onClick={() => { setShiftFilter(shift); setCurrentPage(1) }}>{shiftLabels[shift]}<span>{shift ? shiftCounts[shift] ?? 0 : employees.length}</span></button>)}</div>
      <div className="table-scroll"><table><thead><tr><th>EMPLEADO</th><th>TURNO</th><th>INGRESO</th><th>SALIDA</th><th>ESTADO</th><th>CRÉDITO FRANCO</th></tr></thead><tbody>{visibleRows.map(({ employee, record }) => <tr key={employee.id}><td><div className="employee-cell"><div className="avatar avatar-small">{employee.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><b>{employee.name}</b></div></td><td>{employee.shift}</td><td><input aria-label={`Ingreso de ${employee.name}`} className="table-input time-input" type="time" disabled={record.status !== 'Presente'} value={record.entry} onChange={(event) => updateAttendance(employee.id, 'entry', event.target.value)} /></td><td><input aria-label={`Salida de ${employee.name}`} className="table-input time-input" type="time" disabled={record.status !== 'Presente'} value={record.exit} onChange={(event) => updateAttendance(employee.id, 'exit', event.target.value)} /></td><td><select aria-label={`Estado de ${employee.name}`} className="table-input status-input" value={record.status} onChange={(event) => updateAttendance(employee.id, 'status', event.target.value)}>{attendanceStatuses.map((status) => {
        const isFranco = status === 'Franco'
        const disabled = isFranco && !record.tieneFrancoDisponible
        return <option key={status} value={status} disabled={disabled}>{isFranco ? (record.tieneFrancoDisponible ? '⭐ Franco (Compensatorio)' : 'Franco (Bloqueado - Sin sábado)') : status}</option>
      })}</select></td><td><span style={{ fontSize: '12px', color: record.tieneFrancoDisponible ? '#177447' : '#738a9c' }}>{record.detalleFranco || (record.tieneFrancoDisponible ? '⭐ Franco disponible' : 'Sin crédito')}</span></td></tr>)}</tbody></table>{rows.length === 0 && <div className="empty-state">No encontramos personas con esa búsqueda.</div>}</div>
      <Pagination currentPage={currentPage} pageCount={Math.max(1, Math.ceil(rows.length / pageSize))} total={rows.length} onChange={setCurrentPage} />
      <div className="table-footer"><span>Los francos compensatorios se validan contra el sábado inmediatamente anterior.</span><span>{rows.length} personas en la lista</span></div>
    </div> : <HistoryPanel employees={employees} attendance={attendance} />}
  </>
}

function HistoryPanel({ employees, attendance }) {
  const [currentPage, setCurrentPage] = useState(1)
  const [status, setStatus] = useState('Todos los estados')
  const [search, setSearch] = useState('')
  const [fromDate, setFromDate] = useState(demoDate)
  const [toDate, setToDate] = useState(demoDate)
  const [shift, setShift] = useState('Todos los turnos')
  const [type, setType] = useState('Todos los tipos')
  const records = attendance.map((record) => {
    const employee = employees.find((item) => item.id === record.id)
    return { record, employee, date: record.date ?? demoDate, type: record.type ?? 'Ordinaria' }
  }).filter(({ record, employee, date, type: rowType }) => employee
    && date >= fromDate && date <= toDate
    && (status === 'Todos los estados' || record.status === status)
    && (shift === 'Todos los turnos' || employee.shift === shift)
    && (type === 'Todos los tipos' || rowType === type)
    && employee.name.toLowerCase().includes(search.toLowerCase()))
  const visibleRecords = records.slice((currentPage - 1) * pageSize, currentPage * pageSize)
  const resetPage = (setter) => (event) => { setter(event.target.value); setCurrentPage(1) }
  return <div className="table-panel"><div className="table-toolbar history-toolbar">
    <label className="field-search"><Search size={16} /><input aria-label="Buscar empleado en el historial" value={search} onChange={resetPage(setSearch)} placeholder="Buscar empleado" /></label>
    <label className="date-field"><CalendarDays size={16} /><input aria-label="Desde" type="date" value={fromDate} onChange={resetPage(setFromDate)} /></label>
    <label className="date-field"><CalendarDays size={16} /><input aria-label="Hasta" type="date" value={toDate} onChange={resetPage(setToDate)} /></label>
    <label className="select-filter"><SlidersHorizontal size={15} /><select aria-label="Filtrar por turno" value={shift} onChange={resetPage(setShift)}><option>Todos los turnos</option><option>Mañana</option><option>Siesta</option><option>Tarde</option><option>Noche</option></select></label>
    <label className="select-filter"><select aria-label="Filtrar por tipo de jornada" value={type} onChange={resetPage(setType)}><option>Todos los tipos</option><option>Ordinaria</option><option>Sábado</option><option>Evento</option></select></label>
    <label className="select-filter"><select aria-label="Filtrar por estado" value={status} onChange={resetPage(setStatus)}><option>Todos los estados</option>{attendanceStatuses.map((item) => <option key={item}>{item}</option>)}</select></label>
  </div><div className="table-scroll"><table><thead><tr><th>FECHA</th><th>JORNADA</th><th>EMPLEADO</th><th>TURNO</th><th>INGRESO</th><th>SALIDA</th><th>ESTADO</th></tr></thead><tbody>{visibleRecords.map(({ record, employee, date, type: rowType }) => <tr key={record.id}><td>{formatDate(date)}</td><td>{rowType}</td><td><b>{employee.name}</b></td><td>{employee.shift}</td><td>{record.entry || '—'}</td><td>{record.exit || '—'}</td><td><StatusBadge value={record.status} /></td></tr>)}</tbody></table>{records.length === 0 && <div className="empty-state">No hay registros para los filtros seleccionados.</div>}</div><Pagination currentPage={currentPage} pageCount={Math.max(1, Math.ceil(records.length / pageSize))} total={records.length} onChange={setCurrentPage} /><div className="table-footer"><span>Historial consultado desde la API.</span><span>{records.length} resultados</span></div></div>
}

function EventsPage({ events, eventConvocations, plannedEventIndexes, onAdd, onPlanEvent }) {
  const [currentPage, setCurrentPage] = useState(1)
  const visibleEvents = events.slice((currentPage - 1) * pageSize, currentPage * pageSize)
  return <>
    <PageHeading eyebrow="AGENDA MUNICIPAL · EVENTOS EXTRAORDINARIOS" title="Eventos" description="Organizá las actividades, turnos operativos y calculá la nómina por scoring." action={<button className="button button-primary" onClick={onAdd}><Plus size={17} /> Crear evento</button>} />
    <div className="event-summary"><div><span className="eyebrow">EN AGENDA</span><b>{events.length}</b></div><div><span className="eyebrow">BORRADORES</span><b>{events.filter((event) => event.state === 'Borrador').length}</b></div><div><span className="eyebrow">PERSONAL CONVOCADO</span><b>{events.reduce((sum, event) => sum + Number(event.people), 0)}</b></div></div>
    <div className="table-panel"><div className="table-toolbar"><span className="toolbar-title">Agenda y planificaciones de eventos</span><span className="results-count">Ordenados por fecha</span></div><div className="table-scroll"><table><thead><tr><th>EVENTO</th><th>FECHA Y HORA</th><th>LUGAR</th><th>PERSONAL</th><th>ASISTENCIA</th><th>ESTADO</th><th>ACCIONES</th></tr></thead><tbody>{visibleEvents.map((event, index) => { const eventIndex = (currentPage - 1) * pageSize + index; const plan = eventConvocations[eventIndex]; const isPlanned = Boolean(plan) || plannedEventIndexes.includes(eventIndex); const attendees = plan?.people ?? []; const present = attendees.filter((person) => person.attendance === 'Presente').length; return <tr key={event.name}><td><b>{event.name}</b></td><td>{formatDate(event.date)}<span className="cell-subtext">{event.time} h</span></td><td>{event.place}</td><td>{plan ? `${attendees.length} convocados` : event.people > 0 ? `${event.people} solicitados` : 'A definir'}</td><td>{plan ? <span className="attendance-summary">{present}/{attendees.length} presentes</span> : '—'}</td><td><StatusBadge value={isPlanned ? 'Confirmado' : event.state} /></td><td><div className="planning-row-actions">{isPlanned ? <><button className="button button-small button-outline" onClick={() => onPlanEvent(eventIndex, false)}>Ver asistencia</button><button className="button button-small button-quiet" disabled={dateHasPassed(event.date)} onClick={() => onPlanEvent(eventIndex, true)}>Editar</button></> : <button className="button button-small button-outline" disabled={dateHasPassed(event.date)} onClick={() => onPlanEvent(eventIndex, false)}>Planificar evento</button>}</div></td></tr> })}</tbody></table></div><Pagination currentPage={currentPage} pageCount={Math.max(1, Math.ceil(events.length / pageSize))} total={events.length} onChange={setCurrentPage} /></div>
  </>
}

function SaturdayPage({ employees, generated, loading, people, workDate, regime, convocation, convocations, onSelectionChange, onGenerate, onToggle, onShift, onRestore, onConfirm }) {
  const [currentPage, setCurrentPage] = useState(1)
  const [confirmedPage, setConfirmedPage] = useState(1)
  const [editing, setEditing] = useState(false)
  const [activeSection, setActiveSection] = useState('confirmed')
  const [planToEdit, setPlanToEdit] = useState(null)
  const isPast = dateHasPassed(workDate)
  const isReadOnly = Boolean(convocation) && !editing
  const shifts = regime === 'Incorporado' ? ['Mañana', 'Tarde', 'Noche'] : ['Mañana', 'Siesta', 'Noche']
  const displayPeople = isReadOnly ? convocation.people : people
  const regimeEmployees = [
    ...displayPeople.filter((person) => person.regime === regime),
    ...employees.filter((employee) => employee.regime === regime && !displayPeople.some((person) => person.id === employee.id)),
  ]
  const visibleEmployees = regimeEmployees.slice((currentPage - 1) * pageSize, currentPage * pageSize)
  const confirmedPlans = Object.values(convocations).sort((first, second) => second.date.localeCompare(first.date))
  const visiblePlans = confirmedPlans.slice((confirmedPage - 1) * pageSize, confirmedPage * pageSize)
  const assignedCount = displayPeople.filter((person) => person.works !== false).length

  const startNewPlanning = () => {
    let candidate = nextSaturdayISO()
    setEditing(false)
    setPlanToEdit(null)
    setActiveSection('create')
    onSelectionChange(candidate, regime)
  }

  const editConfirmedPlan = (plan) => {
    setPlanToEdit(null)
    setEditing(true)
    setCurrentPage(1)
    setActiveSection('create')
    onSelectionChange(plan.date, plan.regime)
    onRestore(plan.date, plan.regime)
  }

  const confirmConvocation = () => {
    onConfirm()
    setEditing(false)
    setActiveSection('confirmed')
  }

  return <>
    <PageHeading eyebrow="ALGORITMO DE ROTACIÓN QUINCENAL (50% DOTACIÓN)" title="Planificación de sábados" description="Generá la nómina de convocados y confirmá formalmente la cobertura." action={activeSection === 'create' && generated && !loading && (convocation && !editing
      ? <button className="button button-outline" disabled={isPast} onClick={() => { onRestore(); setEditing(true) }}><Users size={16} /> {isPast ? 'Fecha finalizada' : 'Editar convocatoria'}</button>
      : <>{convocation && <button className="button button-quiet" onClick={() => { onRestore(); setEditing(false) }}>Cancelar edición</button>}<button className="button button-primary" disabled={isPast} onClick={confirmConvocation}><Check size={16} /> {convocation ? 'Guardar cambios' : 'Confirmar planificación'}</button></>)} />
    <div className="saturday-tabs" role="tablist" aria-label="Secciones de planificación de sábados">
      <button type="button" role="tab" aria-selected={activeSection === 'confirmed'} className={activeSection === 'confirmed' ? 'saturday-tab-active' : ''} onClick={() => setActiveSection('confirmed')}>Planificaciones confirmadas <span>{confirmedPlans.length}</span></button>
      <button type="button" role="tab" aria-selected={activeSection === 'create'} className={activeSection === 'create' ? 'saturday-tab-active' : ''} onClick={() => { if (activeSection !== 'create') startNewPlanning() }}><Plus size={15} /> Crear planificación</button>
    </div>
    {activeSection === 'confirmed' && <section className="saturday-confirmed-section" role="tabpanel">
      <div className="table-panel confirmed-plans-panel"><div className="table-toolbar"><div><span className="toolbar-title">Planificaciones registradas</span><span className="cell-subtext">Consultá las jornadas generadas y guardadas en base de datos.</span></div><span className="results-count">{confirmedPlans.length} {confirmedPlans.length === 1 ? 'jornada' : 'jornadas'}</span></div><div className="table-scroll"><table><thead><tr><th>FECHA</th><th>GRUPO</th><th>ESTADO</th><th>ACCIONES</th></tr></thead><tbody>{visiblePlans.map((plan) => { const planIsPast = dateHasPassed(plan.date); return <tr key={`${plan.date}:${plan.regime}`}><td><b>{formatDate(plan.date)}</b></td><td>{plan.regime}</td><td><StatusBadge value="Confirmado" /></td><td><div className="planning-row-actions"><button className="button button-small button-quiet" disabled={planIsPast} onClick={() => editConfirmedPlan(plan)}>Ver detalle</button></div></td></tr> })}</tbody></table></div>{confirmedPlans.length ? <Pagination currentPage={confirmedPage} pageCount={Math.max(1, Math.ceil(confirmedPlans.length / pageSize))} total={confirmedPlans.length} onChange={setConfirmedPage} /> : <div className="empty-state saturday-no-plans">Todavía no hay planificaciones guardadas. Usá “Crear planificación” para generar con el algoritmo.</div>}</div>
    </section>}
    {activeSection === 'create' && <div className="saturday-create-section" role="tabpanel">
    <div className="saturday-workspace">

      <section className="saturday-setup">
        <div className="saturday-section-heading"><div><span className="eyebrow">CONFIGURACIÓN</span><h2>Prepará la jornada</h2></div><span className="setup-step"><i>1</i> Definí fecha y equipo</span></div>
        <div className="saturday-setup-fields">
          <label className="saturday-field"><span>FECHA DE TRABAJO (SÁBADO)</span><div><CalendarDays size={17} /><input aria-label="Fecha de trabajo" type="date" value={workDate} disabled={loading} onChange={(event) => { setEditing(false); setCurrentPage(1); onSelectionChange(event.target.value, regime) }} /></div></label>
          <label className="saturday-field"><span>GRUPO DE PERSONAL</span><div><Users size={17} /><select aria-label="Grupo de personal" value={regime} disabled={loading} onChange={(event) => { setEditing(false); setCurrentPage(1); onSelectionChange(workDate, event.target.value) }}><option>Incorporado</option></select></div></label>
          {!generated && <button className="button button-primary saturday-generate" disabled={loading || isPast} onClick={() => onGenerate(regime)}>{loading ? <><LoaderCircle className="loading-spinner" size={17} /> Ejecutando algoritmo...</> : <><Activity size={17} /> Generar convocatoria</>}</button>}
        </div>
        {isPast && <p className="event-turn-required">La fecha de esta jornada ya pasó. La convocatoria puede consultarse, pero no modificarse ni generarse.</p>}
      </section>

      <section className="saturday-stats" aria-label="Resumen de planificación">
        <article className="saturday-stat"><span className="saturday-stat-icon"><Users size={17} /></span><div><small>PERSONAL DEL RÉGIMEN</small><b>{regimeEmployees.length}</b><span>personas disponibles</span></div></article>
        <article className="saturday-stat"><span className="saturday-stat-icon"><Clock3 size={17} /></span><div><small>TURNOS A CUBRIR</small><b>{shifts.length}</b><span>{shifts.join(' · ')}</span></div></article>
        <article className="saturday-stat"><span className="saturday-stat-icon"><Check size={17} /></span><div><small>PERSONAL ASIGNADO (50%)</small><b>{generated ? `${assignedCount}/${people.length}` : '—'}</b><span>{generated ? 'asignaciones por algoritmo' : 'pendiente de propuesta'}</span></div></article>
      </section>

      {loading ? <div className="proposal-loading saturday-loading" role="status" aria-live="polite"><LoaderCircle className="loading-spinner" size={34} /><h2>Calculando rotación equitativa...</h2><p>Evaluando la alternancia quincenal y la distribución equitativa de turnos.</p><div className="loading-progress"><span /></div><span className="loading-caption">Consultando historial</span></div> : generated ? <>
        <div className="proposal-banner saturday-proposal-banner"><div><span className="proposal-label">{convocation ? 'CONVOCATORIA CONFIRMADA' : 'PROPUESTA DE DOMINIO · BORRADOR'}</span><b>{assignedCount} personas asignadas automáticamente</b></div><span>{convocation ? `Jornada del ${formatDate(workDate)}${isPast ? ' · Fecha finalizada, solo lectura' : isReadOnly ? ' · Podés editarla hasta la fecha' : ' · Revisá y guardá los cambios'}` : 'Revisá la propuesta preliminar generada por el algoritmo y confirmá cuando esté lista.'}</span></div>
        <div className="shift-columns saturday-shift-columns">{shifts.map((shift) => {
          const shiftPeople = displayPeople.filter((person) => person.works !== false && person.saturdayShift?.toLowerCase().includes(shift.toLowerCase()))
          const shiftHours = shift === 'Noche' ? '18:00 – 24:00' : shift === 'Tarde' || shift === 'Siesta' ? '12:00 – 18:00' : '06:00 – 12:00'
          return <div className={`shift-column saturday-shift-card shift-${shift.toLowerCase()}`} key={shift}>
            <div className="shift-column-heading"><div><span className="shift-card-kicker">TURNO</span><b>{shift}</b><span>{shiftHours}</span></div><span className="count-tag">{shiftPeople.length}</span></div>
            <div className="shift-people-list">{shiftPeople.map((person) => <div className="shift-person" key={person.id}><div className="avatar avatar-small">{person.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><div><b>{person.name}</b><span>{person.funcion}</span></div></div>)}{shiftPeople.length === 0 && <p className="muted-empty">Sin personal en este turno.</p>}</div>
          </div>
        })}</div>
        <div className="table-panel edit-proposal saturday-edit-panel"><div className="table-toolbar"><div><span className="toolbar-title">{isReadOnly ? 'Personal convocado' : 'Ajustar propuesta antes de confirmar'}</span><span className="cell-subtext">{isReadOnly ? 'Detalle de la nómina guardada.' : 'Modificá turnos o alterná francos con motivo justificado (RF12).'}</span></div><span className="results-count">{regimeEmployees.length} personas evaluadas</span></div><div className="table-scroll"><table><thead><tr><th>PERSONA</th><th>RÉGIMEN</th><th>DISPONIBILIDAD</th><th>TURNO ASIGNADO</th></tr></thead><tbody>{visibleEmployees.map((employee) => { const person = displayPeople.find((item) => item.id === employee.id); return <tr key={employee.id}><td><div className="employee-cell"><div className="avatar avatar-small">{employee.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><b>{employee.name}</b></div></td><td><span className={`regime-chip regime-${employee.regime.toLowerCase()}`}>{employee.regime}</span></td><td><label className="toggle-label"><input type="checkbox" disabled={isReadOnly || isPast} checked={person ? person.works !== false : false} onChange={() => person && onToggle(employee.id)} />{person?.works === false || !person ? 'Descansa' : 'Trabaja'}</label></td><td>{person?.works !== false && person ? <select aria-label={`Turno de ${employee.name}`} className="table-input shift-input" disabled={isReadOnly || isPast} value={person.saturdayShift} onChange={(event) => onShift(employee.id, event.target.value)}>{shifts.map((item) => <option key={item}>{item}</option>)}</select> : <span className="rest-label">— Franco quincenal</span>}</td></tr> })}</tbody></table></div><Pagination currentPage={currentPage} pageCount={Math.max(1, Math.ceil(regimeEmployees.length / pageSize))} total={regimeEmployees.length} onChange={setCurrentPage} /></div>
      </> : <section className="saturday-empty"><div className="saturday-empty-icon"><Users size={23} /></div><div><span className="eyebrow">LISTO PARA PLANIFICAR</span><h2>Comenzá a organizar la jornada</h2><p>Seleccioná la fecha y ejecutá el algoritmo de rotación para cubrir el 50% de la dotación activa.</p></div><div className="saturday-empty-steps"><span><i>1</i> Elegí el sábado</span><span><i>2</i> Ejecutá el algoritmo</span><span><i>3</i> Confirmá formalmente</span></div></section>}
    </div>
    </div>}
    {planToEdit && <Modal title={`Planificación · ${formatDate(planToEdit.date)}`} onClose={() => setPlanToEdit(null)}>
      <p className="cell-subtext saturday-modal-subtitle">Personal asignado formalmente a esta jornada</p>
      <div className="saturday-modal-roster"><table><thead><tr><th>PERSONA</th><th>FUNCIÓN</th><th>TURNO</th></tr></thead><tbody>{planToEdit.people.filter((person) => person.works !== false).map((person) => <tr key={person.id}><td><div className="employee-cell"><div className="avatar avatar-small">{person.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><b>{person.name}</b></div></td><td>{person.funcion}</td><td>{person.saturdayShift}</td></tr>)}</tbody></table>{planToEdit.people.length === 0 && <div className="empty-state">Planificación registrada en base de datos.</div>}</div>
      <div className="modal-actions"><button type="button" className="button button-outline" onClick={() => setPlanToEdit(null)}>Cerrar</button></div>
    </Modal>}
  </>
}

function AssignmentPage({ events, employees, selectedEvent, setSelectedEvent, startEditing, generated, loading, selectedPeople, assignmentShifts, turns, turnsConfirmed, convocation, onGenerate, onToggle, onShiftChange, onTurnChange, onTurnsConfirm, onAttendanceChange, onConfirm, go }) {
  const [currentPage, setCurrentPage] = useState(1)
  const [showTurnConfig, setShowTurnConfig] = useState(false)
  const [editingConvocation, setEditingConvocation] = useState(startEditing)
  const [turnForm, setTurnForm] = useState({ name: '', start: '', end: '', people: '1' })
  const [turnError, setTurnError] = useState('')
  const currentEvent = events[selectedEvent]
  const isPast = currentEvent ? dateHasPassed(currentEvent.date) : false
  const isReadOnly = Boolean(convocation) && !editingConvocation
  const canEditConvocation = Boolean(convocation) && !isPast
  const totalTurnPeople = turns.reduce((total, turn) => total + Number(turn.people), 0)
  const canConfirmTurns = turns.length > 0 && totalTurnPeople > 0
  const visibleEmployees = employees.slice((currentPage - 1) * pageSize, currentPage * pageSize)
  
  const updateTurns = (nextTurns) => {
    setTurnError('')
    onTurnChange(selectedEvent, nextTurns)
  }

  const saveTurn = (event) => {
    event.preventDefault()
    if (turnForm.start === turnForm.end) {
      setTurnError('El horario de inicio y fin no pueden ser iguales.')
      return
    }
    const peopleNum = Math.max(1, Number(turnForm.people) || 1)
    updateTurns([...turns, { ...turnForm, id: `${Date.now()}-${turns.length}`, people: peopleNum }])
    setTurnForm({ name: '', start: '', end: '', people: '1' })
  }

  const confirmTurns = () => {
    if (!canConfirmTurns) return
    onTurnsConfirm(selectedEvent)
    setTurnError('')
  }

  const confirmConvocation = () => {
    if (isPast) return
    onConfirm(turns)
    setEditingConvocation(false)
  }

  const cancelConvocationEdit = () => {
    setSelectedEvent(selectedEvent)
    setEditingConvocation(false)
  }

  const handleSelectEvent = (event) => {
    setEditingConvocation(false)
    setCurrentPage(1)
    setShowTurnConfig(false)
    setTurnForm({ name: '', start: '', end: '', people: '1' })
    setSelectedEvent(Number(event.target.value))
  }

  const assignedRoster = convocation?.people ?? []
  const visibleAssignedRoster = assignedRoster.slice((currentPage - 1) * pageSize, currentPage * pageSize)

  return <>
    <PageHeading eyebrow="ALGORITMO DE SCORING PONDERADO (RF16 - RF18)" title="Planificación de eventos" description="Configurá los turnos y revisá el ranking de puntaje ponderado con exclusión de 48 hs de descanso." action={<div className="heading-actions"><button className="button button-outline" onClick={() => go('eventos')}><ArrowLeft size={16} /> Volver a eventos</button>{canEditConvocation && isReadOnly && <button className="button button-outline" onClick={() => setEditingConvocation(true)}><Users size={16} /> Editar convocatoria</button>}{generated && !isReadOnly && <>{convocation && <button className="button button-quiet" onClick={cancelConvocationEdit}>Cancelar edición</button>}<button className="button button-primary" disabled={isPast || loading} onClick={confirmConvocation}><Check size={16} /> {convocation ? 'Guardar cambios' : 'Confirmar convocatoria'}</button></>}{convocation && isPast && <span className="event-lock-label">Fecha finalizada · solo lectura</span>}</div>} />
    <div className="assignment-config"><div className="assignment-select"><label className="eyebrow" htmlFor="event-select">EVENTO</label><select id="event-select" value={selectedEvent} disabled={loading} onChange={handleSelectEvent} >{events.map((item, index) => <option value={index} key={item.name}>{item.name}</option>)}</select><span>{currentEvent ? `${formatDate(currentEvent.date)} · ${currentEvent.place}` : 'Creá un evento para comenzar.'}</span></div><div className="capacity-box"><span className="eyebrow">DOTACIÓN TOTAL</span><b>{totalTurnPeople}</b><span>personas a convocar</span></div><button className="button button-outline" disabled={loading || isPast || Boolean(convocation)} aria-expanded={showTurnConfig} onClick={() => setShowTurnConfig((open) => !open)}><Plus size={16} /> {showTurnConfig ? 'Ocultar turnos' : 'Configurar turnos'}</button></div>
    {showTurnConfig && <section className="event-turn-config" aria-label="Configuración de turnos">
      <div className="event-turn-heading"><div><span className="eyebrow">COBERTURA DEL EVENTO</span><h2>Turnos disponibles</h2></div><span>{turns.length} configurados</span></div>
      {turns.length > 0 && <div className="event-turn-list">{turns.map((turn) => <article className="event-turn-item" key={turn.id}><div><b>{turn.name}</b><span>{turn.start}–{turn.end}</span></div><strong>{turn.people} <small>personas</small></strong><button type="button" className="event-turn-remove" disabled={loading} aria-label={`Quitar turno ${turn.name}`} onClick={() => updateTurns(turns.filter((item) => item.id !== turn.id))}><X size={15} /></button></article>)}</div>}
      {turns.length === 0 && <p className="event-turn-empty">Todavía no hay turnos configurados. Agregá los horarios y la cantidad de personal necesarios para este evento.</p>}
      <p className="event-turn-capacity">Turnos configurados: {turns.length} · Dotación total a cubrir: {totalTurnPeople} personas</p>
      <form className="event-turn-form" onSubmit={saveTurn}>
        <label>Turno<input required disabled={loading || isPast} value={turnForm.name} onChange={(event) => setTurnForm({ ...turnForm, name: event.target.value })} placeholder="Ej. Refuerzo Nocturno" /></label>
        <label>Desde<input required disabled={loading || isPast} type="time" value={turnForm.start} onChange={(event) => setTurnForm({ ...turnForm, start: event.target.value })} /></label>
        <label>Hasta<input required disabled={loading || isPast} type="time" value={turnForm.end} onChange={(event) => setTurnForm({ ...turnForm, end: event.target.value })} /></label>
        <label>Personal requerido<input required disabled={loading || isPast} type="number" min="1" step="1" value={turnForm.people} onChange={(event) => setTurnForm({ ...turnForm, people: event.target.value })} /></label>
        <button className="button button-primary" disabled={loading || isPast}><Plus size={15} /> Agregar turno</button>
      </form>
      {turnError && <p className="event-turn-error" role="alert">{turnError}</p>}
      <div className="event-turn-actions">{turnsConfirmed && <span className="event-turn-confirmed"><Check size={15} /> Turnos confirmados</span>}<button type="button" className="button button-primary" disabled={!canConfirmTurns || turnsConfirmed || loading || isPast} onClick={confirmTurns}><Check size={15} /> {turnsConfirmed ? 'Turnos confirmados' : 'Confirmar turnos'}</button></div>
    </section>}
    {!turnsConfirmed && !generated && !loading && !isPast && <p className="event-turn-required">Configurá y confirmá los turnos para habilitar el algoritmo de scoring.</p>}
    {isPast && !convocation && <p className="event-turn-required">La fecha del evento ya pasó. No se puede generar ni editar una convocatoria.</p>}
    {loading ? <div className="proposal-loading" role="status" aria-live="polite"><LoaderCircle className="loading-spinner" size={34} /><h2>Ejecutando Scoring Ponderado...</h2><p>El backend está calculando factores de historial, rotación, sábados y asistencia semanal.</p><div className="loading-progress"><span /></div><span className="loading-caption">Verificando 48 hs de descanso de guardia nocturna</span></div> : isReadOnly ? <div className="table-panel"><div className="table-toolbar"><div><span className="toolbar-title">Convocatoria confirmada</span><span className="cell-subtext">{currentEvent?.name} · {currentEvent ? formatDate(currentEvent.date) : ''}</span></div><span className="selected-count">{assignedRoster.length} convocados</span></div><div className="table-scroll"><table><thead><tr><th>PERSONA</th><th>FUNCIÓN</th><th>TURNO</th><th>HORARIO</th><th>ASISTENCIA</th></tr></thead><tbody>{visibleAssignedRoster.map((person) => { const turn = convocation.turns?.find((item) => item.id === convocation.assignments[person.id]); return <tr key={person.id}><td><div className="employee-cell"><div className="avatar avatar-small">{person.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><b>{person.name}</b></div></td><td>{person.funcion}</td><td>{turn?.name ?? 'Guardia'}</td><td>{turn ? `${turn.start}–${turn.end}` : '09:00 - 17:00'}</td><td><select aria-label={`Asistencia de ${person.name}`} className="table-input status-input" value={person.attendance ?? 'Pendiente'} onChange={(event) => onAttendanceChange(selectedEvent, person.id, event.target.value)}>{convocationAttendanceStatuses.map((status) => <option key={status}>{status}</option>)}</select></td></tr> })}</tbody></table></div><Pagination currentPage={currentPage} pageCount={Math.max(1, Math.ceil(assignedRoster.length / pageSize))} total={assignedRoster.length} onChange={setCurrentPage} /></div> : !generated ? <div className="empty-workspace"><div className="empty-icon"><Users size={23} /></div><h2>Prepará una convocatoria</h2><p>El algoritmo de scoring calculará la prioridad de cada agente y excluirá a quienes estén en descanso obligatorio.</p><button className="button button-primary" disabled={!turnsConfirmed || isPast} onClick={onGenerate}>Generar convocatoria <ChevronRight size={16} /></button></div> : <div className="table-panel"><div className="table-toolbar"><div><span className="toolbar-title">{convocation ? 'Editar convocatoria' : 'Personal sugerido por Scoring'}</span><span className="cell-subtext">{convocation ? 'Actualizá el equipo convocado y guardá los cambios.' : 'Calculado con la fórmula matemática de 4 factores de SIGEP.'}</span></div><span className="selected-count">{selectedPeople.length} seleccionados</span></div><div className="table-scroll"><table><thead><tr><th>PERSONA</th><th>FUNCIÓN</th><th>ESTADO</th><th>TURNO SUGERIDO</th>{convocation && <th>ASISTENCIA</th>}<th>CONVOCAR</th></tr></thead><tbody>{visibleEmployees.map((employee, index) => { const currentPerson = convocation?.people.find((person) => person.id === employee.id); return <tr key={employee.id}><td><div className="employee-cell"><div className="avatar avatar-small">{employee.name.split(' ').map((part) => part[0]).slice(0, 2).join('')}</div><b>{employee.name}</b></div></td><td>{employee.funcion}</td><td><StatusBadge value={selectedPeople.includes(employee.id) ? 'Presente' : 'Borrador'} /></td><td><select className="table-input shift-input" aria-label={`Turno sugerido para ${employee.name}`} value={assignmentShifts[employee.id] ?? ''} disabled={isPast} onChange={(event) => onShiftChange(employee.id, event.target.value)}>{turns.length ? turns.map((turn) => <option key={turn.id} value={turn.id}>{turn.name} · {turn.start}–{turn.end}</option>) : <option value="1">Guardia Evento</option>}</select></td>{convocation && currentPerson && <td><select aria-label={`Asistencia de ${employee.name}`} className="table-input status-input" value={currentPerson.attendance ?? 'Pendiente'} onChange={(event) => onAttendanceChange(selectedEvent, employee.id, event.target.value)}>{convocationAttendanceStatuses.map((status) => <option key={status}>{status}</option>)}</select></td>}<td><input className="checkbox-input" type="checkbox" disabled={isPast} checked={selectedPeople.includes(employee.id)} onChange={() => onToggle(employee.id)} aria-label={`Convocar a ${employee.name}`} /></td></tr> })}</tbody></table></div><Pagination currentPage={currentPage} pageCount={Math.max(1, Math.ceil(employees.length / pageSize))} total={employees.length} onChange={setCurrentPage} /><div className="table-footer"><span>Puntajes y descanso de 48 hs evaluados automáticamente.</span><span>{employees.length} personas evaluadas</span></div></div>}
  </>
}

function Pagination({ currentPage, pageCount, total, onChange }) {
  if (total === 0) return null
  const firstItem = (currentPage - 1) * pageSize + 1
  const lastItem = Math.min(currentPage * pageSize, total)
  return <nav className="pagination" aria-label="Paginación de la tabla">
    <span>Mostrando {firstItem}–{lastItem} de {total}</span>
    <div><button className="pagination-button" aria-label="Página anterior" disabled={currentPage === 1} onClick={() => onChange(currentPage - 1)}><ChevronLeft size={15} /></button><span className="pagination-page">Página {currentPage} de {pageCount}</span><button className="pagination-button" aria-label="Página siguiente" disabled={currentPage === pageCount} onClick={() => onChange(currentPage + 1)}><ChevronRight size={15} /></button></div>
  </nav>
}

function Modal({ title, onClose, children }) {
  return <div className="modal-scrim" role="presentation" onMouseDown={(event) => event.target === event.currentTarget && onClose()}><section className="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title"><div className="modal-heading"><h2 id="modal-title">{title}</h2><button className="icon-button" aria-label="Cerrar" onClick={onClose}><X size={19} /></button></div>{children}</section></div>
}

export default App
