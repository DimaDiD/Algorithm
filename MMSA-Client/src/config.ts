const BASE_URL = '/api/';
export const CONTACT_EMAIL = process.env.REACT_APP_CONTACT_EMAIL?.trim() || null;
export const APP_VERSION = process.env.REACT_APP_VERSION?.trim().slice(0, 7) || "dev";
export default BASE_URL;
