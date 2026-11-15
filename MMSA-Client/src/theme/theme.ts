import type { ThemeConfig } from "antd";

export const colors = {
  primary: "#5B3FE0",
  primaryDark: "#3F27B0",
  primarySoft: "#ECE8FF",
  ink: "#17142B",
  text: "#3B3852",
  muted: "#5B5873",
  border: "#E6E3F2",
  borderStrong: "#D9D4F2",
  background: "#F6F5FB",
  surface: "#FFFFFF",
  danger: "#B42318",
};

export const appTheme: ThemeConfig = {
  token: {
    colorPrimary: colors.primary,
    colorLink: colors.primary,
    colorText: colors.ink,
    colorTextSecondary: colors.muted,
    colorBorder: colors.borderStrong,
    colorBorderSecondary: colors.border,
    colorBgLayout: colors.background,
    colorError: colors.danger,
    fontFamily: "'Manrope', system-ui, -apple-system, 'Segoe UI', sans-serif",
    fontSize: 15,
    borderRadius: 12,
    borderRadiusLG: 20,
    controlHeight: 40,
    controlHeightLG: 48,
  },
  components: {
    Button: {
      primaryShadow: "none",
      defaultShadow: "none",
      dangerShadow: "none",
    },
    Segmented: {
      itemSelectedBg: colors.primary,
      itemSelectedColor: "#FFFFFF",
      trackBg: "#F3F1FA",
    },
    Modal: {
      borderRadiusLG: 24,
    },
  },
};
