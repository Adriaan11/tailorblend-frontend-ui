/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.razor',
    './Shared/**/*.razor',
    './Components/**/*.razor',
    './App.razor',
    './_Imports.razor'
  ],

  // Use [data-theme="dark"] attribute selector for dark mode (existing system)
  darkMode: ['class', '[data-theme="dark"]'],

  theme: {
    extend: {
      // Map CSS variables to Tailwind colors
      colors: {
        'tb': {
          'accent': 'var(--tb-accent)',
          'accent-strong': 'var(--tb-accent-strong)',
          'accent-soft': 'var(--tb-accent-soft)',
          'indigo': 'var(--tb-indigo)',
          'indigo-soft': 'var(--tb-indigo-soft)',
          'gray': {
            '50': 'var(--tb-gray-50)',
            '75': 'var(--tb-gray-75)',
            '100': 'var(--tb-gray-100)',
            '200': 'var(--tb-gray-200)',
            '300': 'var(--tb-gray-300)',
            '400': 'var(--tb-gray-400)',
            '500': 'var(--tb-gray-500)',
            '600': 'var(--tb-gray-600)',
            '700': 'var(--tb-gray-700)',
            '800': 'var(--tb-gray-800)',
            '900': 'var(--tb-gray-900)',
          },
        },
        'text': {
          'primary': 'var(--text-primary)',
          'secondary': 'var(--text-secondary)',
          'muted': 'var(--text-muted)',
          'disabled': 'var(--text-disabled)',
        },
        'bg': {
          'surface': 'var(--bg-surface)',
          'elevated': 'var(--bg-elevated)',
          'app': 'var(--bg-app)',
        },
      },

      // Map spacing scale (--tb-space-* variables)
      spacing: {
        'tb-2': 'var(--tb-space-2)',
        'tb-4': 'var(--tb-space-4)',
        'tb-8': 'var(--tb-space-8)',
        'tb-12': 'var(--tb-space-12)',
        'tb-16': 'var(--tb-space-16)',
        'tb-20': 'var(--tb-space-20)',
        'tb-24': 'var(--tb-space-24)',
        'tb-32': 'var(--tb-space-32)',
        'tb-40': 'var(--tb-space-40)',
        'tb-48': 'var(--tb-space-48)',
        'tb-64': 'var(--tb-space-64)',
      },

      // Map border radius (--tb-radius-* variables)
      borderRadius: {
        'tb-xs': 'var(--tb-radius-xs)',
        'tb-sm': 'var(--tb-radius-sm)',
        'tb': 'var(--tb-radius)',
        'tb-lg': 'var(--tb-radius-lg)',
        'tb-xl': 'var(--tb-radius-xl)',
        'tb-pill': 'var(--tb-radius-pill)',
      },

      // Map shadows (--tb-shadow-* variables)
      boxShadow: {
        'tb-sm': 'var(--tb-shadow-sm)',
        'tb': 'var(--tb-shadow)',
        'tb-lg': 'var(--tb-shadow-lg)',
        'tb-xl': 'var(--tb-shadow-xl)',
        'tb-accent-sm': 'var(--shadow-accent-sm)',
        'tb-accent-md': 'var(--shadow-accent-md)',
        'tb-accent-lg': 'var(--shadow-accent-lg)',
      },

      // Font families
      fontFamily: {
        'sans': 'var(--tb-font-sans)',
        'mono': 'var(--tb-font-mono)',
      },

      // Z-index scale
      zIndex: {
        'dropdown': 'var(--z-dropdown)',
        'sticky': 'var(--z-sticky)',
        'fixed': 'var(--z-fixed)',
        'modal-backdrop': 'var(--z-modal-backdrop)',
        'modal': 'var(--z-modal)',
        'popover': 'var(--z-popover)',
        'tooltip': 'var(--z-tooltip)',
      },
    },
  },

  plugins: [],
}
