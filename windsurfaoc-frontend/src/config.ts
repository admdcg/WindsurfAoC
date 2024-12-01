export const config = {
    // En desarrollo usamos localhost, en producción la URL de producción
    apiBaseUrl: import.meta.env.PROD ? 'https://aventofcode.intrasoft.es/backend/api' : 'http://localhost:5071/api'
}

export default config;
