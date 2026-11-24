export const fabricxTheme = {
  dark: false,
  colors: {
    background: '#F6F7F9',
    surface: '#FFFFFF',
    'surface-variant': '#EEF0F3',
    'on-surface-variant': '#3A4250',

    primary: '#4F6BED',
    secondary: '#3A4250',

    'primary-lighten-4': '#EDF0FD',
    'primary-lighten-3': '#D3DAFB',
    'primary-lighten-2': '#B0BCF7',
    'primary-lighten-1': '#8497F2',
    'primary-darken-1': '#4057C5',
    'primary-darken-2': '#32439D',
    'primary-darken-3': '#232F75',

    success: '#2E7D32',

    warning: '#B23E00',
    error: '#C62828',
    info: '#0B6BCB',

    'on-background': '#1B1F24',
    'on-surface': '#1B1F24',
    'text-muted': '#5E6470',
  },
  variables: {
    'border-color': '#1B1F24',
    'border-opacity': 0.1,
    'high-emphasis-opacity': 1,
    'medium-emphasis-opacity': 0.72,
    'hover-opacity': 0.04,
  },
}

export const fabricxDarkTheme = {
  dark: true,
  colors: {
    background: '#14171C',
    surface: '#1C2027',
    'surface-variant': '#262B33',
    'on-surface-variant': '#C3C8D0',

    primary: '#8497F2',
    secondary: '#AEB5C1',

    'primary-lighten-4': '#2A3366',
    'primary-lighten-3': '#334083',
    'primary-lighten-2': '#4A5BC0',
    'primary-lighten-1': '#6F84EE',
    'primary-darken-1': '#A9B6F6',
    'primary-darken-2': '#C9D1FA',
    'primary-darken-3': '#E3E8FD',

    success: '#66BB6A',
    warning: '#FF9E5E',
    error: '#F26B6B',
    info: '#5AA9F5',

    'on-primary': '#14171C',
    'on-secondary': '#14171C',
    'on-success': '#14171C',
    'on-warning': '#14171C',
    'on-error': '#14171C',
    'on-info': '#14171C',

    'on-background': '#E6E8EB',
    'on-surface': '#E6E8EB',
    'text-muted': '#9BA2AE',
  },
  variables: {
    'border-color': '#FFFFFF',
    'border-opacity': 0.1,
    'high-emphasis-opacity': 1,
    'medium-emphasis-opacity': 0.72,
    'hover-opacity': 0.06,
  },
}

export const fabricxDefaults = {
  VDataTable: { density: 'compact', hover: true, itemsPerPageOptions: [5, 10, 25, 50, 100], pageText: '{0}-{1} de {2}' },
  VBtn: { variant: 'flat', rounded: 'lg', class: 'text-none' },
  VChip: { variant: 'flat', size: 'small', rounded: 'lg' },
  VTextField: { variant: 'outlined', density: 'compact', color: 'primary' },
  VSelect: { variant: 'outlined', density: 'compact', color: 'primary' },
  VAutocomplete: { variant: 'outlined', density: 'compact', color: 'primary' },
  VCard: { rounded: 'lg', elevation: 0, border: true },
  VNavigationDrawer: { color: 'surface', border: 'e' },
  VAppBar: { color: 'surface', flat: true, border: 'b' },
}
