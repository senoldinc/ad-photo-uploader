# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AD Photo Manager is a React + TypeScript application for managing Active Directory user photos with canvas-based circular cropping. The UI is entirely in Turkish.

## Development Commands

```bash
npm run dev          # Start dev server (default: http://localhost:5173)
npm run build        # Production build
npm run preview      # Preview production build
npm run lint         # Run ESLint
npm run type-check   # TypeScript type checking without emit
```

## Architecture

### Tech Stack
- **React 19** with TypeScript strict mode
- **Vite 8** as build tool
- **Tailwind CSS v3** for styling
- **React Query** for state management
- **Canvas API** for image cropping

### Path Aliases
Configured in both `tsconfig.json` and `vite.config.ts`:
- `@/*` → `src/*`
- `@components/*` → `src/components/*`
- `@hooks/*` → `src/hooks/*`
- `@lib/*` → `src/lib/*`
- `@context/*` → `src/context/*`
- `@types` → `src/types`
- `@pages/*` → `src/pages/*`

### Key Components

**App.tsx**: Main application with user grid, search/filter, and upload orchestration. Contains mock data for 15 AD users.

**CropModal**: Canvas-based circular image cropper with:
- 300×300px output resolution
- Drag-to-reposition functionality (mouse + touch)
- Zoom control (0.5× to 3.0×)
- JPEG quality slider (30% to 100%)
- Real-time file size calculation with 100KB limit enforcement
- Visual feedback for oversized files

**useImageCrop hook**: Manages crop state (position, scale, quality) and renders the cropped image to canvas with circular clipping path.

**Theme System**: Light/dark mode via ThemeContext with localStorage persistence.

### Image Upload Constraints

- **Allowed formats**: JPG/JPEG only (`image/jpeg`, `image/jpg`)
- **Source file limit**: 500 KB
- **Output limit**: 100 KB (enforced in UI, prevents upload if exceeded)
- **Output dimensions**: 300×300 px circular crop
- **Quality range**: 0.3 to 1.0 (30% to 100%)

### Mock Data

15 Turkish AD users defined in `App.tsx` with structure:
```typescript
interface SimpleUser {
  id: string
  displayName: string      // Turkish names
  employeeId: string       // Format: "SİC-00001"
  title: string           // Turkish job titles
  organization: string    // Turkish org names
  department: string      // Turkish dept names
  email: string
  photo: string | null    // Base64 data URL after upload
}
```

## Language & Localization

All UI text must be in Turkish. Key terms:
- "Fotoğraf Kırp" (Crop Photo)
- "Yakınlaştırma" (Zoom)
- "Kalite" (Quality)
- "Dosya Boyutu" (File Size)
- "Onayla & Yükle" (Confirm & Upload)

## Design System

- **Primary color**: `#e74c3c` (red accent)
- **Typography**:
  - Display/headings: Bebas Neue (via Google Fonts)
  - Body: DM Sans (via Google Fonts)
- **Theme**: CSS variables defined in ThemeContext for light/dark modes

## Future Integration Points

The codebase includes infrastructure for:
- OIDC authentication (`lib/auth.ts`, `react-oidc-context`)
- Backend API integration (`lib/api.ts` with Axios)
- Real AD user data (currently using mock data)

Environment variables are prefixed with `VITE_` (e.g., `VITE_API_BASE_URL`, `VITE_USE_MOCK`).
