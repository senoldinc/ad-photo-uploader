import React, { useState, useEffect, useRef } from 'react'
import { ZoomIn, ZoomOut, SlidersHorizontal, AlertTriangle, CheckCircle2, Loader2, Move } from 'lucide-react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Slider } from '@/components/ui/slider'
import { cn } from '@/lib/utils'
import { useImageCrop } from '@/hooks/useImageCrop'

interface CropModalProps {
  file: File | null
  isOpen: boolean
  onConfirm: (base64: string, kb: number) => void
  onCancel: () => void
}

export const CropModal: React.FC<CropModalProps> = ({ file, isOpen, onConfirm, onCancel }) => {
  const [imageUrl, setImageUrl] = useState<string>('')
  const [isProcessing, setIsProcessing] = useState(false)
  const [isDragging, setIsDragging] = useState(false)

  // Drag state: stores the pointer start position and the position at drag start
  const dragOrigin = useRef<{ clientX: number; clientY: number; posX: number; posY: number } | null>(null)

  const { cropState, outputSizeKb, isOversized, cropImage, updatePosition, updateScale, updateQuality } =
    useImageCrop(file)

  useEffect(() => {
    if (file) {
      const url = URL.createObjectURL(file)
      setImageUrl(url)
      return () => URL.revokeObjectURL(url)
    }
  }, [file])

  // ── Mouse drag ────────────────────────────────────────────────────────────
  const handleMouseDown = (e: React.MouseEvent) => {
    e.preventDefault()
    setIsDragging(true)
    dragOrigin.current = {
      clientX: e.clientX,
      clientY: e.clientY,
      posX: cropState.position.x,
      posY: cropState.position.y,
    }
  }

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!dragOrigin.current) return
    const dx = e.clientX - dragOrigin.current.clientX
    const dy = e.clientY - dragOrigin.current.clientY
    updatePosition(dragOrigin.current.posX + dx, dragOrigin.current.posY + dy)
  }

  const handleMouseUp = () => {
    setIsDragging(false)
    dragOrigin.current = null
  }

  // ── Touch drag ────────────────────────────────────────────────────────────
  const handleTouchStart = (e: React.TouchEvent) => {
    const t = e.touches[0]
    setIsDragging(true)
    dragOrigin.current = {
      clientX: t.clientX,
      clientY: t.clientY,
      posX: cropState.position.x,
      posY: cropState.position.y,
    }
  }

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!dragOrigin.current) return
    e.preventDefault()
    const t = e.touches[0]
    const dx = t.clientX - dragOrigin.current.clientX
    const dy = t.clientY - dragOrigin.current.clientY
    updatePosition(dragOrigin.current.posX + dx, dragOrigin.current.posY + dy)
  }

  const handleTouchEnd = () => {
    setIsDragging(false)
    dragOrigin.current = null
  }

  // ── Confirm ───────────────────────────────────────────────────────────────
  const handleConfirm = async () => {
    setIsProcessing(true)
    try {
      const { base64, sizeKb } = await cropImage()
      onConfirm(base64, sizeKb)
    } finally {
      setIsProcessing(false)
    }
  }

  const sizePercent = Math.min((outputSizeKb / 100) * 100, 100)

  return (
    <Dialog open={isOpen} onOpenChange={open => !open && onCancel()}>
      <DialogContent className="max-w-lg w-full gap-0 p-0 overflow-hidden border-[3px] border-border shadow-brutal-lg">
        <DialogHeader className="px-6 pt-6 pb-4 border-b-[3px] border-border bg-accent">
          <DialogTitle className="font-display text-3xl tracking-tight text-accent-foreground">
            FOTOĞRAF KIRP
          </DialogTitle>
          <p className="text-xs text-accent-foreground/70 tracking-wide mt-1 font-bold uppercase">
            300 × 300 px · Maks 100 KB · JPG
          </p>
        </DialogHeader>

        <div className="px-6 py-6 space-y-6 bg-card">
          {/* Circular preview with drag */}
          <div className="flex flex-col items-center gap-3">
            <div
              className={cn(
                "relative w-64 h-64 rounded-full overflow-hidden border-[4px] bg-muted select-none shadow-brutal-lg transition-brutal",
                isDragging ? "cursor-grabbing border-primary scale-[1.02]" : "cursor-grab border-border"
              )}
              onMouseDown={handleMouseDown}
              onMouseMove={handleMouseMove}
              onMouseUp={handleMouseUp}
              onMouseLeave={handleMouseUp}
              onTouchStart={handleTouchStart}
              onTouchMove={handleTouchMove}
              onTouchEnd={handleTouchEnd}
            >
              {imageUrl && (
                <img
                  src={imageUrl}
                  alt="Önizleme"
                  className="w-full h-full object-cover pointer-events-none"
                  draggable={false}
                  style={{
                    transform: `translate(${cropState.position.x}px, ${cropState.position.y}px) scale(${cropState.scale})`,
                    transformOrigin: 'center',
                    transition: isDragging ? 'none' : 'transform 0.1s ease-out',
                  }}
                />
              )}
              {/* Guide ring */}
              <div className="absolute inset-0 rounded-full border-[3px] border-dashed border-white/30 pointer-events-none" />
            </div>

            {/* Drag hint */}
            <div className="flex items-center gap-2 px-3 py-2 bg-muted border-[2px] border-border">
              <Move className="w-4 h-4 text-foreground" strokeWidth={2.5} />
              <span className="text-xs font-bold text-foreground uppercase">Sürükle & Konumlandır</span>
            </div>
          </div>

          {/* Zoom */}
          <div className="space-y-3 p-4 bg-muted border-[3px] border-border">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <ZoomIn className="w-5 h-5 text-foreground" strokeWidth={2.5} />
                <span className="font-bold text-sm text-foreground uppercase">Yakınlaştırma</span>
              </div>
              <span className="text-lg font-display text-foreground tabular-nums">
                {cropState.scale.toFixed(1)}×
              </span>
            </div>
            <div className="flex items-center gap-3">
              <ZoomOut className="w-4 h-4 text-muted-foreground flex-shrink-0" strokeWidth={2.5} />
              <Slider
                min={0.5} max={3} step={0.1}
                value={[cropState.scale]}
                onValueChange={([v]) => updateScale(v)}
                className="flex-1"
              />
              <ZoomIn className="w-4 h-4 text-foreground flex-shrink-0" strokeWidth={2.5} />
            </div>
          </div>

          {/* Quality */}
          <div className="space-y-3 p-4 bg-muted border-[3px] border-border">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <SlidersHorizontal className="w-5 h-5 text-foreground" strokeWidth={2.5} />
                <span className="font-bold text-sm text-foreground uppercase">Kalite</span>
              </div>
              <span className="text-lg font-display text-foreground tabular-nums">
                {Math.round(cropState.quality * 100)}%
              </span>
            </div>
            <Slider
              min={0.3} max={1} step={0.05}
              value={[cropState.quality]}
              onValueChange={([v]) => updateQuality(v)}
            />
          </div>

          {/* Size indicator */}
          <div className={cn(
            "p-4 border-[3px] transition-colors shadow-brutal",
            isOversized
              ? "border-destructive bg-destructive/10"
              : "border-success bg-success/10"
          )}>
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-2">
                {isOversized
                  ? <AlertTriangle className="w-5 h-5 text-destructive" strokeWidth={2.5} />
                  : <CheckCircle2 className="w-5 h-5 text-success" strokeWidth={2.5} />
                }
                <span className="text-sm font-bold text-foreground uppercase">Dosya Boyutu</span>
              </div>
              <span className={cn(
                "text-2xl font-display tracking-tight tabular-nums",
                isOversized ? "text-destructive" : "text-success"
              )}>
                {outputSizeKb} KB
              </span>
            </div>
            <div className="w-full h-3 bg-background border-[2px] border-border overflow-hidden">
              <div
                className={cn(
                  "h-full transition-all duration-300",
                  isOversized ? "bg-destructive" : "bg-success"
                )}
                style={{ width: `${sizePercent}%` }}
              />
            </div>
            {isOversized && (
              <p className="text-xs text-destructive mt-2 font-bold uppercase">
                ⚠ Limit aşıldı — kaliteyi düşürün
              </p>
            )}
          </div>
        </div>

        <DialogFooter className="px-6 pb-6 pt-4 gap-3 flex-row border-t-[3px] border-border bg-muted">
          <Button
            variant="outline"
            onClick={onCancel}
            disabled={isProcessing}
            className="flex-1 h-12 font-bold text-sm uppercase border-[3px] shadow-brutal hover:shadow-brutal-hover transition-brutal"
          >
            İPTAL
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={isOversized || isProcessing}
            className="flex-1 h-12 font-display text-sm tracking-tight border-[3px] shadow-brutal hover:shadow-brutal-hover transition-brutal disabled:opacity-50"
          >
            {isProcessing ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin" strokeWidth={2.5} />
                <span className="ml-2">İŞLENİYOR...</span>
              </>
            ) : (
              'ONAYLA & YÜKLE'
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
