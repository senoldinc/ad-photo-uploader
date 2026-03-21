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
    <div className="px-6 py-4 border-b border-border space-y-3">
      {/* Search row */}
      <div className="flex items-center gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground pointer-events-none" />
          <Input
            value={search}
            onChange={e => handleSearchChange(e.target.value)}
            placeholder="Ad, sicil veya departman ara…"
            className="pl-9 h-8 text-sm"
          />
          {search && (
            <button
              onClick={() => handleSearchChange('')}
              className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          )}
        </div>

        {resultCount !== undefined && (
          <span className="text-xs text-muted-foreground whitespace-nowrap">
            <span className="font-semibold text-foreground">{resultCount}</span> sonuç
          </span>
        )}

        {hasFilters && (
          <Button variant="ghost" size="sm" onClick={handleClear} className="h-8 text-xs text-muted-foreground">
            Temizle
          </Button>
        )}
      </div>

      {/* Department filters */}
      <div className="flex flex-wrap gap-1.5">
        {departments.map(dept => (
          <button
            key={dept}
            onClick={() => handleDeptToggle(dept)}
            className={cn(
              "inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium transition-all duration-150 border",
              department === dept
                ? "bg-primary text-primary-foreground border-primary"
                : "bg-background text-muted-foreground border-border hover:border-foreground/30 hover:text-foreground"
            )}
          >
            {dept}
          </button>
        ))}
      </div>
    </div>
  )
}
