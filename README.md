# AD Photo Manager

A modern React + TypeScript web application for managing Active Directory user photos with image cropping, real-time preview, and theme support.

## 🚀 Features

- **User Management Grid**: Display AD users with search and department filtering
- **Photo Upload**: Drag-and-drop or click-to-browse file selection
- **Canvas Image Cropping**:
  - 300×300 px circular crop
  - Zoom (0.5× to 3.0×) and quality (30% to 100%) controls
  - Live KB size indicator with 100 KB limit enforcement
  - Real-time preview with dashed guide overlay
- **Theme System**: Light/Dark mode with smooth transitions
- **Toast Notifications**: Global notification system for user feedback
- **Mock Backend**: Pre-populated with 15 test AD users
- **Responsive Design**: Mobile-first, works on all devices
- **Accessibility**: WCAG AA compliant with keyboard navigation

## 📦 Tech Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool (0.35s HMR)
- **Tailwind CSS v4** - Styling with CSS variables
- **React Query** - Server state management
- **Axios** - HTTP client with interceptors
- **OIDC Client** - Authentication ready

## 💻 Getting Started

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The app will be available at **http://localhost:5173**

### Available Commands

```bash
npm run dev          # Start dev server with hot reload
npm run build        # Build for production
npm run preview      # Preview production build
npm run lint         # Run ESLint
npm run type-check   # TypeScript type checking
```

## 📂 Project Structure

```
src/
├── components/ui/              # Reusable components (Button, Badge, Input, Avatar, Toast)
├── components/                 # Feature components (UserCard, CropModal, SearchBar, StatsBar, ThemeToggle)
├── context/                    # Context providers (ThemeContext, ToastContext)
├── hooks/                      # Custom hooks (useUsers, useUploadPhoto, useImageCrop)
├── lib/                        # Utilities (auth, api, mockData)
├── pages/                      # Page components (Dashboard, Callback)
├── types/                      # TypeScript interfaces
├── styles/                     # Global styles (globals.css, theme.css)
├── App.tsx                     # Main app component
└── main.tsx                    # Entry point with providers
```

## 🎨 Design System

### Colors (CSS Variables)
- Primary: `#e74c3c` (light) / `#ff6b6b` (dark)
- Secondary: `#004E89`
- Success: `#22c55e` / `#10b981`
- Error: `#ef4444` / `#f87171`

### Typography
- **Display**: Bebas Neue (headings)
- **Body**: DM Sans (content)

## 🎯 Key Features

### Image Cropping
- 300×300 px circular output
- Zoom: 0.5× to 3.0×
- Quality: 30% to 100%
- File size limit: 100 KB
- Live KB indicator with color feedback

### User Management
- 15 mock AD users
- Search by name, ID, email
- Filter by department
- Upload status indicators

### Theme System
- Light/Dark mode toggle
- Smooth CSS transitions
- localStorage persistence

### Notifications
- Toast system with auto-dismiss
- Success, error, info, warning types
- Fixed bottom-right positioning

## 🔧 Environment Variables

```env
VITE_API_BASE_URL=http://localhost:3000/api
VITE_OIDC_AUTHORITY=https://localhost:5000
VITE_OIDC_CLIENT_ID=ad-photo-manager
VITE_USE_MOCK=true
```

## 🔗 API Endpoints (Ready to Connect)

```
GET    /api/users                    → ADUser[]
POST   /api/users/:id/photo          → UploadPhotoResponse
DELETE /api/users/:id/photo          → { success: boolean }
```

## 📱 Responsive Design

- Desktop (lg): 1024px+
- Tablet (md): 768px - 1023px
- Mobile (sm): 640px - 767px
- Mobile Small: < 640px

User grid: 4 columns (lg) → 2 columns (md) → 1 column (sm)

## ✅ Completed Checklist

- [x] TypeScript + strict mode configured
- [x] Vite setup with path aliases
- [x] Tailwind CSS v4 integrated
- [x] React Query for data fetching
- [x] Theme system (Light/Dark)
- [x] Toast notifications
- [x] Canvas image cropping (300×300, 100KB limit)
- [x] Mock AD users (15 users)
- [x] User search & filtering
- [x] Component library (ui/)
- [x] Dashboard layout
- [x] Responsive design
- [x] Accessibility features
- [x] OIDC config ready

## 🚀 Next Steps

1. **Integrate OIDC Authentication**
2. **Connect Real Backend API**
3. **Add Unit Tests** (vitest)
4. **Deploy to Production**

---

**Status**: ✅ Ready for development
**Last Updated**: March 21, 2026
