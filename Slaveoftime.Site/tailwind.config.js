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
      themes: [
        {
          light: {
            ...require("daisyui/src/colors/themes")["[data-theme=light]"],
            primary: "rgb(20 184 166)",
          },
          dark: {
            ...require("daisyui/src/colors/themes")["[data-theme=dark]"],
            primary: "rgb(20 184 166)",
          }
        }
      ]
  }
}
