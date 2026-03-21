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
      <DialogContent className="max-w-md w-full gap-0 p-0 overflow-hidden">
        <DialogHeader className="px-6 pt-6 pb-4 border-b border-border">
          <DialogTitle className="font-display text-2xl tracking-wider">
            FOTOĞRAF KIRP
          </DialogTitle>
          <p className="text-xs text-muted-foreground tracking-wide mt-1">
            300 × 300 px · Maks 100 KB · JPG
          </p>
        </DialogHeader>

        <div className="px-6 py-5 space-y-5">
          {/* Circular preview with drag */}
          <div className="flex flex-col items-center gap-2">
            <div
              className={cn(
                "relative w-56 h-56 rounded-full overflow-hidden border-2 bg-secondary select-none",
                isDragging ? "cursor-grabbing border-primary" : "cursor-grab border-border"
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
              <div className="absolute inset-0 rounded-full border-2 border-dashed border-white/20 pointer-events-none" />
            </div>

            {/* Drag hint */}
            <div className="flex items-center gap-1 text-xs text-muted-foreground">
              <Move className="w-3 h-3" />
              <span>Görseli sürükleyerek konumlandırın</span>
            </div>
          </div>

          {/* Zoom */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                <ZoomIn className="w-3.5 h-3.5" />
                <span className="font-medium">Yakınlaştırma</span>
              </div>
              <span className="text-xs font-semibold text-foreground tabular-nums">
                {cropState.scale.toFixed(1)}×
              </span>
            </div>
            <div className="flex items-center gap-3">
              <ZoomOut className="w-3.5 h-3.5 text-muted-foreground flex-shrink-0" />
              <Slider
                min={0.5} max={3} step={0.1}
                value={[cropState.scale]}
                onValueChange={([v]) => updateScale(v)}
              />
              <ZoomIn className="w-3.5 h-3.5 text-muted-foreground flex-shrink-0" />
            </div>
          </div>

          {/* Quality */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                <SlidersHorizontal className="w-3.5 h-3.5" />
                <span className="font-medium">Kalite</span>
              </div>
              <span className="text-xs font-semibold text-foreground tabular-nums">
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
            "rounded-md p-3 border transition-colors",
            isOversized
              ? "border-destructive/50 bg-destructive/5"
              : "border-border bg-secondary/50"
          )}>
            <div className="flex items-center justify-between mb-2">
              <div className="flex items-center gap-1.5">
                {isOversized
                  ? <AlertTriangle className="w-3.5 h-3.5 text-destructive" />
                  : <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
                }
                <span className="text-xs font-medium text-foreground">Dosya Boyutu</span>
              </div>
              <span className={cn(
                "text-sm font-bold font-display tracking-wider tabular-nums",
                isOversized ? "text-destructive" : "text-emerald-500"
              )}>
                {outputSizeKb} KB
              </span>
            </div>
            <div className="w-full h-1 rounded-full bg-border overflow-hidden">
              <div
                className={cn(
                  "h-full rounded-full transition-all duration-300",
                  isOversized ? "bg-destructive" : "bg-emerald-500"
                )}
                style={{ width: `${sizePercent}%` }}
              />
            </div>
            {isOversized && (
              <p className="text-xs text-destructive mt-1.5">
                Limit aşıldı — kaliteyi düşürün
              </p>
            )}
          </div>
        </div>

        <DialogFooter className="px-6 pb-6 pt-2 gap-2 flex-row">
          <Button
            variant="outline"
            onClick={onCancel}
            disabled={isProcessing}
            className="flex-1"
          >
            İptal
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={isOversized || isProcessing}
            className="flex-1 font-display tracking-widest"
          >
            {isProcessing
              ? <><Loader2 className="w-4 h-4 animate-spin" /> İşleniyor…</>
              : 'ONAYLA & YÜKLE'
            }
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
