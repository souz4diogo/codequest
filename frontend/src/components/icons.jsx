// Ícones SVG (stroke 1.75, 24x24) — substitui emojis por um único family/estilo consistente.
// Cada ícone aceita size/className como qualquer outro componente de ícone (ex.: lucide-react).
function Icon({ size = 20, children, ...props }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.75}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      {...props}
    >
      {children}
    </svg>
  );
}

export const IconDashboard = (p) => (
  <Icon {...p}>
    <rect x="3" y="3" width="7" height="9" rx="1.5" />
    <rect x="14" y="3" width="7" height="5" rx="1.5" />
    <rect x="14" y="12" width="7" height="9" rx="1.5" />
    <rect x="3" y="16" width="7" height="5" rx="1.5" />
  </Icon>
);

export const IconTree = (p) => (
  <Icon {...p}>
    <circle cx="6" cy="6" r="2.5" />
    <circle cx="18" cy="6" r="2.5" />
    <circle cx="12" cy="13" r="2.5" />
    <circle cx="12" cy="20.5" r="2" />
    <path d="M6 8.5v3a2 2 0 0 0 2 2h1.5M18 8.5v3a2 2 0 0 0-2 2h-1.5M12 15.5v2.5" />
  </Icon>
);

export const IconTarget = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="8.5" />
    <circle cx="12" cy="12" r="4.5" />
    <circle cx="12" cy="12" r="0.8" fill="currentColor" />
  </Icon>
);

export const IconCode = (p) => (
  <Icon {...p}>
    <path d="M9 8 4.5 12 9 16M15 8l4.5 4-4.5 4" />
  </Icon>
);

export const IconClipboardCheck = (p) => (
  <Icon {...p}>
    <rect x="5" y="4" width="14" height="17" rx="2" />
    <path d="M9 4V3a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v1" />
    <path d="M9 13.5l1.8 1.8L15.5 11" />
  </Icon>
);

export const IconTimer = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="13" r="8" />
    <path d="M12 9v4l2.5 2.5M9.5 2h5M12 2v2.5" />
  </Icon>
);

export const IconShoppingBag = (p) => (
  <Icon {...p}>
    <path d="M6 8h12l-1 12.5a1.5 1.5 0 0 1-1.5 1.5H8.5A1.5 1.5 0 0 1 7 20.5L6 8Z" />
    <path d="M9 8V6a3 3 0 0 1 6 0v2" />
  </Icon>
);

export const IconMessageCircle = (p) => (
  <Icon {...p}>
    <path d="M21 12a8.5 8.5 0 1 1-3.4-6.8L21 4l-1 4.3A8.4 8.4 0 0 1 21 12Z" />
    <path d="M8 12h.01M12 12h.01M16 12h.01" />
  </Icon>
);

export const IconLogOut = (p) => (
  <Icon {...p}>
    <path d="M9 21H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3" />
    <path d="M16 17l5-5-5-5M21 12H9" />
  </Icon>
);

export const IconFlame = (p) => (
  <Icon {...p}>
    <path d="M12 22c4-1 6-4 6-7.5 0-2.5-1.3-4-2.5-5.3.2 2-.6 3-1.5 3.3C15 9 13.7 6.5 11 4c.3 2.5-1 4-2.5 6-1.2 1.5-2 3-2 5C6.5 18 8.5 21 12 22Z" />
  </Icon>
);

export const IconCoins = (p) => (
  <Icon {...p}>
    <ellipse cx="9" cy="8" rx="6" ry="3.5" />
    <path d="M3 8v4c0 1.9 2.7 3.5 6 3.5s6-1.6 6-3.5V8" />
    <path d="M3 12v4c0 1.9 2.7 3.5 6 3.5s6-1.6 6-3.5v-4" />
  </Icon>
);

export const IconZap = (p) => (
  <Icon {...p}>
    <path d="M13 2 4.5 13.5H11L10 22l8.5-11.5H12l1-8.5Z" />
  </Icon>
);

export const IconShield = (p) => (
  <Icon {...p}>
    <path d="M12 3l7 3v6c0 5-3.5 7.8-7 9-3.5-1.2-7-4-7-9V6l7-3Z" />
  </Icon>
);

export const IconSwords = (p) => (
  <Icon {...p}>
    <path d="M5 20 19 6M14.5 3.5 20 4l.5 5.5-4 .3-1.6-1.6z" />
    <path d="M19 20 5 6M9.5 3.5 4 4l-.5 5.5 4 .3 1.6-1.6z" />
    <path d="M9 15l-4 4M15 15l4 4" />
  </Icon>
);

export const IconCheckCircle = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="9" />
    <path d="M8.5 12.5l2.3 2.3L16 9.5" />
  </Icon>
);

export const IconXCircle = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="9" />
    <path d="M9.5 9.5l5 5M14.5 9.5l-5 5" />
  </Icon>
);

export const IconLock = (p) => (
  <Icon {...p}>
    <rect x="5" y="11" width="14" height="9" rx="2" />
    <path d="M8 11V7a4 4 0 0 1 8 0v4" />
  </Icon>
);

export const IconChevronRight = (p) => (
  <Icon {...p}>
    <path d="M9 5l7 7-7 7" />
  </Icon>
);

export const IconStar = ({ filled, ...p }) => (
  <Icon {...p} fill={filled ? "currentColor" : "none"}>
    <path d="M12 3.5l2.7 5.6 6.1.9-4.4 4.3 1 6.1L12 17.4l-5.4 2.9 1-6-4.4-4.4 6.1-.9L12 3.5Z" />
  </Icon>
);

export const IconTrophy = (p) => (
  <Icon {...p}>
    <path d="M8 4h8v5a4 4 0 0 1-8 0V4Z" />
    <path d="M8 5H5.5A1.5 1.5 0 0 0 4 6.5c0 2 1.5 3.2 3.3 3.5M16 5h2.5A1.5 1.5 0 0 1 20 6.5c0 2-1.5 3.2-3.3 3.5" />
    <path d="M10 15.5h4M12 13v3M9 20.5h6l-.6-3H9.6z" />
  </Icon>
);

export const IconSparkles = (p) => (
  <Icon {...p}>
    <path d="M11 2.5 12.3 7l4.5 1.3-4.5 1.3L11 14l-1.3-4.4L5.2 8.3l4.5-1.3z" />
    <path d="M18 14.5l.8 2.3 2.3.8-2.3.8-.8 2.3-.8-2.3-2.3-.8 2.3-.8z" />
  </Icon>
);

export const IconLoader = (p) => (
  <Icon {...p} className={["icon-spin", p.className].filter(Boolean).join(" ")}>
    <path d="M12 3a9 9 0 1 0 9 9" />
  </Icon>
);

export const IconWand = (p) => (
  <Icon {...p}>
    <path d="M4 20 15 9M13 3l1.3 2.7L17 7l-2.7 1.3L13 11l-1.3-2.7L9 7l2.7-1.3z" />
    <path d="M18 13l.7 1.5L20 15l-1.3.7L18 17l-.7-1.3L16 15l1.3-.5z" />
  </Icon>
);

export const IconPlus = (p) => (
  <Icon {...p}>
    <path d="M12 5v14M5 12h14" />
  </Icon>
);

export const IconActivity = (p) => (
  <Icon {...p}>
    <path d="M3 12h4l2 7 4-14 2 7h6" />
  </Icon>
);

export const IconRadar = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="9" />
    <circle cx="12" cy="12" r="5" />
    <circle cx="12" cy="12" r="1.2" fill="currentColor" />
    <path d="M12 12 18 6" />
  </Icon>
);

export const IconClock = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="9" />
    <path d="M12 7v5l3.5 2" />
  </Icon>
);

export const IconPlayCircle = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="9" />
    <path d="M10 8.5v7l6-3.5z" fill="currentColor" stroke="none" />
  </Icon>
);

export const IconWalker = (p) => (
  <Icon {...p}>
    <circle cx="13" cy="4" r="1.8" fill="currentColor" stroke="none" />
    <path d="M13 6.5 10 9v4l-2.5 6M13 6.5l3 2.5-1 5 3.5 4.5M13 6.5l-4 1.5v4l4-1" />
  </Icon>
);

export const IconInbox = (p) => (
  <Icon {...p}>
    <path d="M4 12.5 6.5 5h11l2.5 7.5" />
    <rect x="4" y="12.5" width="16" height="7" rx="2" />
    <path d="M9 12.5a3 3 0 0 0 6 0" />
  </Icon>
);
