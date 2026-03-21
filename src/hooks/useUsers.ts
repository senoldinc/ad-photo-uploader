import { useQuery } from '@tanstack/react-query'
import { mockUsers } from '@lib/mockData'
import { ADUser } from '@types'

export const useUsers = (search?: string, department?: string) => {
  return useQuery({
    queryKey: ['users', { search, department }],
    queryFn: async (): Promise<ADUser[]> => {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 300))

      // Filter mockUsers based on search and department
      let filtered = [...mockUsers]

      if (search) {
        const searchLower = search.toLowerCase()
        filtered = filtered.filter(
          (user) =>
            user.displayName.toLowerCase().includes(searchLower) ||
            user.employeeId.toLowerCase().includes(searchLower) ||
            user.email.toLowerCase().includes(searchLower)
        )
      }

      if (department) {
        filtered = filtered.filter((user) => user.department === department)
      }

      return filtered
    },
    staleTime: 1000 * 60 * 5, // 5 minutes
  })
}
