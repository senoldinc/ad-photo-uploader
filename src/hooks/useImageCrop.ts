import { useState, useCallback, useEffect, useRef } from 'react'
import { CropState } from '@types'

const CANVAS_SIZE = 300
// Must match the preview circle in CropModal (w-56 = 224px)
const PREVIEW_SIZE = 224
const MAX_FILE_SIZE_KB = 100
const MIN_SCALE = 0.5
const MAX_SCALE = 3
const MIN_QUALITY = 0.3
const MAX_QUALITY = 1

/**
 * Draw image into a square canvas replicating CSS object-cover + scale + translate.
 * No circular clip — output is a clean square JPEG.
 * The circular preview in the modal is achieved purely via CSS border-radius.
 */
function drawCroppedImage(
  ctx: CanvasRenderingContext2D,
  img: HTMLImageElement,
  state: CropState
) {
  const { scale, position } = state
  const cx = CANVAS_SIZE / 2
  const ratio = CANVAS_SIZE / PREVIEW_SIZE

  // object-cover: fill CANVAS_SIZE×CANVAS_SIZE keeping aspect ratio
  const aspect = img.naturalWidth / img.naturalHeight
  let dw: number, dh: number
  if (aspect >= 1) { dh = CANVAS_SIZE; dw = dh * aspect }
  else             { dw = CANVAS_SIZE; dh = dw / aspect }

  // Replicate CSS transform: translate(posX, posY) scale(scale), origin center
  ctx.save()
  ctx.translate(cx + position.x * ratio, cx + position.y * ratio)
  ctx.scale(scale, scale)
  ctx.drawImage(img, (dw - CANVAS_SIZE) / -2 - cx, (dh - CANVAS_SIZE) / -2 - cx, dw, dh)
  ctx.restore()
}

export const useImageCrop = (file: File | null) => {
  const [cropState, setCropState] = useState<CropState>({
    position: { x: 0, y: 0 },
    scale: 1,
    quality: 0.8,
  })
  const [outputSizeKb, setOutputSizeKb] = useState<number>(0)
  const imgRef = useRef<HTMLImageElement | null>(null)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Load image once per file
  useEffect(() => {
    setCropState({ position: { x: 0, y: 0 }, scale: 1, quality: 0.8 })
    setOutputSizeKb(0)
    imgRef.current = null

    if (!file) return
    const url = URL.createObjectURL(file)
    const img = new Image()
    img.onload = () => { imgRef.current = img; recalcSize({ position: { x: 0, y: 0 }, scale: 1, quality: 0.8 }, img) }
    img.src = url
    return () => URL.revokeObjectURL(url)
  }, [file])

  // Debounced live size recalculation
  function recalcSize(state: CropState, img?: HTMLImageElement) {
    const target = img ?? imgRef.current
    if (!target) return

    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      const canvas = document.createElement('canvas')
      canvas.width = CANVAS_SIZE
      canvas.height = CANVAS_SIZE
      const ctx = canvas.getContext('2d')
      if (!ctx) return
      drawCroppedImage(ctx, target, state)
      const base64 = canvas.toDataURL('image/jpeg', state.quality)
      setOutputSizeKb(Math.round((base64.length * 0.75) / 1024))
    }, 80)
  }

  // Recalculate whenever cropState changes
  useEffect(() => {
    recalcSize(cropState)
  }, [cropState])

  const cropImage = useCallback((): Promise<{ base64: string; sizeKb: number }> => {
    return new Promise((resolve, reject) => {
      if (!file) { reject(new Error('No file')); return }
      const img = imgRef.current
      if (!img) { reject(new Error('Image not loaded')); return }

      const canvas = document.createElement('canvas')
      canvas.width = CANVAS_SIZE
      canvas.height = CANVAS_SIZE
      const ctx = canvas.getContext('2d')
      if (!ctx) { reject(new Error('No canvas context')); return }

      drawCroppedImage(ctx, img, cropState)
      const base64 = canvas.toDataURL('image/jpeg', cropState.quality)
      const sizeKb = Math.round((base64.length * 0.75) / 1024)
      setOutputSizeKb(sizeKb)
      resolve({ base64, sizeKb })
    })
  }, [file, cropState])

  const updatePosition = useCallback((x: number, y: number) => {
    setCropState(prev => ({ ...prev, position: { x, y } }))
  }, [])

  const updateScale = useCallback((scale: number) => {
    setCropState(prev => ({ ...prev, scale: Math.max(MIN_SCALE, Math.min(MAX_SCALE, scale)) }))
  }, [])

  const updateQuality = useCallback((quality: number) => {
    setCropState(prev => ({ ...prev, quality: Math.max(MIN_QUALITY, Math.min(MAX_QUALITY, quality)) }))
  }, [])

  return {
    cropState,
    outputSizeKb,
    isOversized: outputSizeKb > MAX_FILE_SIZE_KB && outputSizeKb > 0,
    cropImage,
    updatePosition,
    updateScale,
    updateQuality,
  }
}
