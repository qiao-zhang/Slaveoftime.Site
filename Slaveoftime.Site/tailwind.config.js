/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.{html,fs}"],
  darkMode: 'media',
  theme: {
      extend: {},
  },
  plugins: [
    require('daisyui'), 
    require('@tailwindcss/typography')
  ],
  daisyui: {
      themes: ["light", "dark"]
  }
}
