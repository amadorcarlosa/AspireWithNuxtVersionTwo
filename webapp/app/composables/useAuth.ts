/**
 * @file useAuth.ts
 * @module WebApp.UI.Composables.Auth
 * @description
 * Authentication composable providing reactive user state and auth helpers.
 * Mirrors Blazor's AuthenticationStateProvider pattern for Vue/Nuxt.
 * 
 * Key features:
 * - Reactive user state shared across components via useState
 * - SSR-compatible user fetching
 * - Role and claim checking (equivalent to Blazor's [Authorize(Roles="...")])
 * - Automatic redirect preservation on login
 */

interface AuthClaim {
  type: string
  value: string
}

interface AuthUser {
  name: string
  claims: AuthClaim[]
}

export const useAuth = () => {
  // Shared state across all component instances (survives HMR too)
  const user = useState<AuthUser | null>('auth-user', () => null)
  const pending = useState('auth-pending', () => true)

  /**
   * Fetch current user from backend. Safe to call multiple times;
   * the cookie is sent automatically.
   */
  const fetchUser = async (): Promise<AuthUser | null> => {
    try {
      const data = await $fetch<AuthUser>('/api/auth/user', {
        credentials: 'include'
      })
      user.value = data
      return data
    } catch {
      user.value = null
      return null
    } finally {
      pending.value = false
    }
  }

  /**
   * Redirect to login. Preserves current route for post-login redirect.
   */
  const login = (returnUrl?: string) => {
    const redirect = returnUrl ?? useRequestURL().pathname
    navigateTo(`/api/auth/login?returnUrl=${encodeURIComponent(redirect)}`, {
      external: true
    })
  }

  /**
   * Sign out and redirect to home.
   */
  const logout = () => {
    navigateTo('/api/auth/logout', { external: true })
  }

  /**
   * Check if user has a specific role.
   * Equivalent to Blazor's: [Authorize(Roles = "Teacher")]
   * 
   * @example
   * if (hasRole('Teacher')) { ... }
   */
  const hasRole = (role: string): boolean => {
    return user.value?.claims.some(
      c => (c.type === 'role' || c.type.endsWith('/role')) && c.value === role
    ) ?? false
  }

  /**
   * Check if user has a specific claim.
   * Equivalent to Blazor's: policy.RequireClaim("permission", "lessons.edit")
   * 
   * @example
   * if (hasClaim('permission', 'lessons.edit')) { ... }
   */
  const hasClaim = (type: string, value?: string): boolean => {
    return user.value?.claims.some(c => {
      const typeMatch = c.type === type || c.type.endsWith(`/${type}`)
      return typeMatch && (value === undefined || c.value === value)
    }) ?? false
  }

  return {
    // State (readonly to prevent accidental mutation)
    user: readonly(user),
    pending: readonly(pending),
    isAuthenticated: computed(() => !!user.value),

    // Actions
    fetchUser,
    login,
    logout,

    // Authorization checks
    hasRole,
    hasClaim
  }
}