import { useState } from "react"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { CheckCircle2, AlertTriangle, X, Wifi } from "lucide-react"
import { ThemeProvider } from "@/context/ThemeContext"
import { ThemeToggle } from "@/components/ThemeToggle"
import { StatsBar } from "@/components/StatsBar"
import { SearchBar } from "@/components/SearchBar"
import { UserCard } from "@/components/UserCard"
import { CropModal } from "@/components/CropModal"
import { cn } from "@/lib/utils"

// ─── Types ────────────────────────────────────────────────────────────────────
interface SimpleUser {
  id: string
  displayName: string
  employeeId: string
  title: string
  organization: string
  department: string
  email: string
  photo: string | null
}

interface Toast {
  id: string
  message: string
  type: "success" | "error"
}

// ─── Mock Data ────────────────────────────────────────────────────────────────
const INITIAL_USERS: SimpleUser[] = [
  { id:"1",  displayName:"Ahmet Yılmaz",     employeeId:"SİC-00001", title:"Yazılım Mimarı",        department:"Bilgi İşlem",     organization:"Merkez Operasyon",    email:"a.yilmaz@sirket.com",   photo:null },
  { id:"2",  displayName:"Zeynep Kara",      employeeId:"SİC-00002", title:"Proje Yöneticisi",      department:"PMO",             organization:"Strateji & Planlama", email:"z.kara@sirket.com",     photo:null },
  { id:"3",  displayName:"Murat Demir",      employeeId:"SİC-00003", title:"Kıdemli Analist",       department:"Finans",          organization:"Mali İşler",          email:"m.demir@sirket.com",    photo:null },
  { id:"4",  displayName:"Selin Çelik",      employeeId:"SİC-00004", title:"İK Uzmanı",             department:"İK",              organization:"İnsan Kaynakları",    email:"s.celik@sirket.com",    photo:null },
  { id:"5",  displayName:"Burak Aydın",      employeeId:"SİC-00005", title:"Ağ Yöneticisi",         department:"Altyapı",         organization:"Bilgi İşlem",         email:"b.aydin@sirket.com",    photo:null },
  { id:"6",  displayName:"Elif Şahin",       employeeId:"SİC-00006", title:"UX Tasarımcısı",        department:"Dijital",         organization:"Ürün & Tasarım",      email:"e.sahin@sirket.com",    photo:null },
  { id:"7",  displayName:"Burcu Şahin",      employeeId:"SİC-00007", title:"İK Müdürü",             department:"İK",              organization:"İnsan Kaynakları",    email:"burcu.s@sirket.com",    photo:null },
  { id:"8",  displayName:"İbrahim Göl",      employeeId:"SİC-00008", title:"Finans Müdürü",         department:"Finans",          organization:"Mali İşler",          email:"i.gol@sirket.com",      photo:null },
  { id:"9",  displayName:"Sinem Özkan",      employeeId:"SİC-00009", title:"Pazarlama Uzmanı",      department:"Pazarlama",       organization:"Ticari",              email:"s.ozkan@sirket.com",    photo:null },
  { id:"10", displayName:"Haktan Barlı",     employeeId:"SİC-00010", title:"Satış Müdürü",          department:"Satış",           organization:"Ticari",              email:"h.barli@sirket.com",    photo:null },
  { id:"11", displayName:"Elif Kırtıl",      employeeId:"SİC-00011", title:"QA Mühendisi",          department:"Kalite",          organization:"Merkez Operasyon",    email:"e.kirtil@sirket.com",   photo:null },
  { id:"12", displayName:"Murat Koçak",      employeeId:"SİC-00012", title:"Kıdemli Tasarımcı",     department:"Dijital",         organization:"Ürün & Tasarım",      email:"m.kocak@sirket.com",    photo:null },
  { id:"13", displayName:"Leila Emiroğlu",   employeeId:"SİC-00013", title:"Veri Analisti",         department:"Analitik",        organization:"Strateji & Planlama", email:"l.emiroglu@sirket.com", photo:null },
  { id:"14", displayName:"Deniz Akbaş",      employeeId:"SİC-00014", title:"Güvenlik Mühendisi",    department:"Altyapı",         organization:"Bilgi İşlem",         email:"d.akbas@sirket.com",    photo:null },
  { id:"15", displayName:"Gökçe Sonar",      employeeId:"SİC-00015", title:"İçerik Yöneticisi",     department:"Pazarlama",       organization:"Ticari",              email:"g.sonar@sirket.com",    photo:null },
]

const ALLOWED_TYPES = ["image/jpeg", "image/jpg"]
const MAX_SOURCE_KB = 500
const queryClient = new QueryClient()

// ─── Toast Component ──────────────────────────────────────────────────────────
function ToastItem({ toast, onRemove }: { toast: Toast; onRemove: (id: string) => void }) {
  return (
    <div className={cn(
      "flex items-start gap-3 px-4 py-3 border-[3px] border-border shadow-brutal-lg animate-slide-in bg-card max-w-sm w-full",
      toast.type === "success" && "border-l-[6px] border-l-success",
      toast.type === "error" && "border-l-[6px] border-l-destructive"
    )}>
      <div className="flex-shrink-0 mt-0.5">
        {toast.type === "success"
          ? <CheckCircle2 className="w-5 h-5 text-success" strokeWidth={2.5} />
          : <AlertTriangle className="w-5 h-5 text-destructive" strokeWidth={2.5} />
        }
      </div>
      <p className="text-sm text-foreground flex-1 leading-snug font-medium">{toast.message}</p>
      <button
        onClick={() => onRemove(toast.id)}
        className="flex-shrink-0 text-muted-foreground hover:text-foreground transition-colors mt-0.5 p-1 hover:bg-muted rounded"
      >
        <X className="w-4 h-4" strokeWidth={2.5} />
      </button>
    </div>
  )
}

// ─── Inner App ────────────────────────────────────────────────────────────────
function AppContent() {
  const [users, setUsers] = useState<SimpleUser[]>(INITIAL_USERS)
  const [search, setSearch] = useState("")
  const [department, setDepartment] = useState("")
  const [selectedUser, setSelectedUser] = useState<SimpleUser | null>(null)
  const [pendingFile, setPendingFile] = useState<File | null>(null)
  const [uploadingId, setUploadingId] = useState<string | null>(null)
  const [uploaded, setUploaded] = useState<Set<string>>(new Set())
  const [toasts, setToasts] = useState<Toast[]>([])

  // Derived
  const departments = [...new Set(users.map(u => u.department))].sort()
  const filtered = users.filter(u => {
    const q = search.toLowerCase()
    const matchSearch = !q ||
      u.displayName.toLowerCase().includes(q) ||
      u.employeeId.toLowerCase().includes(q) ||
      u.department.toLowerCase().includes(q)
    const matchDept = !department || u.department === department
    return matchSearch && matchDept
  })

  // Toasts
  const addToast = (message: string, type: "success" | "error" = "success") => {
    const id = Math.random().toString(36).slice(2)
    setToasts(p => [...p, { id, message, type }])
    setTimeout(() => setToasts(p => p.filter(t => t.id !== id)), 3500)
  }
  const removeToast = (id: string) => setToasts(p => p.filter(t => t.id !== id))

  // Upload flow
  const handlePhotoSelect = (user: SimpleUser, file: File) => {
    if (!ALLOWED_TYPES.includes(file.type)) {
      addToast("Yalnızca JPG/JPEG desteklenir.", "error")
      return
    }
    if (file.size / 1024 > MAX_SOURCE_KB) {
      addToast("Kaynak dosya 500 KB'ı aşıyor.", "error")
      return
    }
    setSelectedUser(user)
    setPendingFile(file)
  }

  const handleCropConfirm = async (base64: string, kb: number) => {
    if (!selectedUser) return
    setUploadingId(selectedUser.id)
    setPendingFile(null)

    await new Promise(r => setTimeout(r, 1200))

    setUsers(prev =>
      prev.map(u => u.id === selectedUser.id ? { ...u, photo: base64 } : u)
    )
    setUploaded(prev => new Set([...prev, selectedUser.id]))
    addToast(`✓ ${selectedUser.displayName} — ${kb} KB yüklendi.`)
    setUploadingId(null)
    setSelectedUser(null)
  }

  const handleCropCancel = () => {
    setPendingFile(null)
    setSelectedUser(null)
  }

  return (
    <div className="min-h-screen bg-background text-foreground flex flex-col">
      {/* ── Header ── */}
      <header className="sticky top-0 z-40 border-b-[3px] border-border bg-background shadow-brutal">
        <div className="flex items-center justify-between h-16 px-6">
          {/* Logo + title */}
          <div className="flex items-center gap-4">
            <div className="p-2 bg-primary border-[3px] border-border shadow-brutal">
              <img
                src="https://www.kocsistem.com.tr/images/koc-logo.png"
                alt="Koç Sistem"
                className="h-6 w-auto object-contain brightness-0 invert"
              />
            </div>
            <div className="hidden sm:block w-[3px] h-8 bg-border" />
            <div className="hidden sm:block">
              <span className="font-display text-xl tracking-tight text-foreground leading-none">
                AD PHOTO MANAGER
              </span>
              <p className="text-[10px] text-muted-foreground tracking-wider uppercase leading-none mt-1 font-bold">
                Active Directory Fotoğraf Yönetimi
              </p>
            </div>
          </div>

          {/* Right side */}
          <div className="flex items-center gap-3">
            <div className="hidden sm:flex items-center gap-2 px-3 py-1.5 bg-success/10 border-[2px] border-success">
              <Wifi className="w-3.5 h-3.5 text-success" strokeWidth={2.5} />
              <span className="text-xs font-bold text-success uppercase">OPERASYONEL</span>
            </div>
            <div className="w-[3px] h-6 bg-border hidden sm:block" />
            <ThemeToggle />
            <div className="w-10 h-10 bg-primary border-[3px] border-border shadow-brutal flex items-center justify-center">
              <span className="font-display text-sm text-primary-foreground tracking-wider">AD</span>
            </div>
          </div>
        </div>
      </header>

      {/* ── Stats ── */}
      <StatsBar
        totalUsers={users.length}
        withPhotoCount={users.filter(u => u.photo).length}
        withoutPhotoCount={users.filter(u => !u.photo).length}
        uploadedThisSessionCount={uploaded.size}
      />

      {/* ── Toolbar ── */}
      <SearchBar
        onSearch={(s, d) => { setSearch(s); setDepartment(d) }}
        departments={departments}
        resultCount={filtered.length}
      />

      {/* ── Grid ── */}
      <main className="flex-1 p-6">
        {filtered.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-5 animate-fade-up">
            {filtered.map(user => (
              <UserCard
                key={user.id}
                user={user}
                onPhotoSelect={handlePhotoSelect}
                justUploaded={uploaded.has(user.id)}
                isLoading={uploadingId === user.id}
              />
            ))}
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center py-32 text-center">
            <div className="w-20 h-20 bg-accent border-[3px] border-border shadow-brutal-lg flex items-center justify-center mb-6">
              <span className="text-4xl">🔍</span>
            </div>
            <h3 className="font-display text-3xl tracking-tight text-foreground mb-3">
              SONUÇ BULUNAMADI
            </h3>
            <p className="text-sm text-muted-foreground max-w-sm leading-relaxed mb-6 font-medium">
              Arama kriterlerinize uygun kullanıcı yok. Başka bir terim veya filtre deneyin.
            </p>
            <button
              onClick={() => { setSearch(""); setDepartment("") }}
              className="px-6 py-3 bg-primary text-primary-foreground font-bold text-sm uppercase border-[3px] border-border shadow-brutal hover:shadow-brutal-hover hover:-translate-y-1 transition-brutal"
            >
              FİLTRELERİ TEMİZLE
            </button>
          </div>
        )}
      </main>

      {/* ── Footer ── */}
      <footer className="border-t-[3px] border-border px-6 py-4 flex items-center justify-between bg-muted">
        <span className="text-[10px] text-foreground tracking-wider uppercase font-bold">
          AD Photo Manager · OIDC Auth Yakında
        </span>
        <div className="flex items-center gap-3">
          <div className="hidden sm:flex items-center gap-2 px-2 py-1 bg-background border-[2px] border-border">
            <span className="text-[10px] text-foreground font-bold">JPG/JPEG</span>
          </div>
          <div className="hidden sm:flex items-center gap-2 px-2 py-1 bg-background border-[2px] border-border">
            <span className="text-[10px] text-foreground font-bold">MAX 100 KB</span>
          </div>
          <div className="hidden sm:flex items-center gap-2 px-2 py-1 bg-background border-[2px] border-border">
            <span className="text-[10px] text-foreground font-bold">300×300 PX</span>
          </div>
        </div>
      </footer>

      {/* ── Crop Modal ── */}
      <CropModal
        file={pendingFile}
        isOpen={!!pendingFile && !!selectedUser}
        onConfirm={handleCropConfirm}
        onCancel={handleCropCancel}
      />

      {/* ── Toasts ── */}
      <div className="fixed bottom-6 right-6 z-50 flex flex-col gap-2 items-end">
        {toasts.map(t => (
          <ToastItem key={t.id} toast={t} onRemove={removeToast} />
        ))}
      </div>
    </div>
  )
}

// ─── Root ─────────────────────────────────────────────────────────────────────
export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AppContent />
      </ThemeProvider>
    </QueryClientProvider>
  )
}
