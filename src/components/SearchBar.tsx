import React, { useState } from 'react'
import { Search, X } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface SearchBarProps {
  onSearch: (search: string, department: string) => void
  departments: string[]
  resultCount?: number
}

export const SearchBar: React.FC<SearchBarProps> = ({ onSearch, departments, resultCount }) => {
  const [search, setSearch] = useState('')
  const [department, setDepartment] = useState('')

  const handleSearchChange = (value: string) => {
    setSearch(value)
    onSearch(value, department)
  }

  const handleDeptToggle = (dept: string) => {
    const newDept = department === dept ? '' : dept
    setDepartment(newDept)
    onSearch(search, newDept)
  }

  const handleClear = () => {
    setSearch('')
    setDepartment('')
    onSearch('', '')
  }

  const hasFilters = search || department

  return (
    <div className="px-6 py-5 border-b-[3px] border-border space-y-4 bg-background">
      {/* Search row */}
      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground pointer-events-none" strokeWidth={2.5} />
          <Input
            value={search}
            onChange={e => handleSearchChange(e.target.value)}
            placeholder="Ad, sicil veya departman ara…"
            className="pl-12 h-12 text-sm font-medium border-[3px] shadow-brutal focus:shadow-brutal-hover transition-brutal"
          />
          {search && (
            <button
              onClick={() => handleSearchChange('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 p-1 text-muted-foreground hover:text-foreground transition-colors"
            >
              <X className="h-4 w-4" strokeWidth={2.5} />
            </button>
          )}
        </div>

        {resultCount !== undefined && (
          <div className="px-4 py-2 bg-accent border-[3px] border-border shadow-brutal">
            <span className="font-display text-lg text-accent-foreground">
              {resultCount}
            </span>
            <span className="text-xs text-accent-foreground/70 ml-1.5 font-bold uppercase">
              SONUÇ
            </span>
          </div>
        )}

        {hasFilters && (
          <Button
            variant="outline"
            size="sm"
            onClick={handleClear}
            className="h-10 px-4 font-bold text-xs border-[3px] shadow-brutal hover:shadow-brutal-hover transition-brutal"
          >
            TEMİZLE
          </Button>
        )}
      </div>

      {/* Department filters */}
      <div className="flex flex-wrap gap-2">
        {departments.map(dept => (
          <button
            key={dept}
            onClick={() => handleDeptToggle(dept)}
            className={cn(
              "inline-flex items-center px-4 py-2 text-xs font-bold uppercase transition-brutal border-[3px]",
              department === dept
                ? "bg-primary text-primary-foreground border-border shadow-brutal-lg"
                : "bg-background text-foreground border-border shadow-brutal hover:shadow-brutal-hover hover:-translate-y-0.5"
            )}
          >
            {dept}
          </button>
        ))}
      </div>
    </div>
  )
}
