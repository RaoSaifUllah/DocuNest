type IconName = 'nest' | 'grid' | 'document' | 'clock' | 'link' | 'trash' | 'settings' | 'shield' | 'arrow' | 'menu' | 'close' | 'sun' | 'moon' | 'monitor' | 'check' | 'alert' | 'folder';

const paths: Record<IconName, string> = {
  nest: 'M6 3h9l4 4v14H6z M14 3v5h5 M10 12h5 M10 16h5',
  grid: 'M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z',
  document: 'M6 3h9l4 4v14H6z M14 3v5h5 M10 12h5 M10 16h5',
  clock: 'M12 8v5l3 2 M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18',
  link: 'M10 13a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-2 2 M14 11a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l2-2',
  trash: 'M3 6h18 M8 6V3h8v3 M5 6l1 15h12l1-15 M10 10v7 M14 10v7',
  settings: 'M9 3h6l1 3 3 1 2 5-2 5-3 1-1 3H9l-1-3-3-1-2-5 2-5 3-1z M12 9a3 3 0 1 0 0 6 3 3 0 0 0 0-6',
  shield: 'M12 3l8 3v6c0 5-8 9-8 9s-8-4-8-9V6z M8 12l3 3 5-6',
  arrow: 'M5 12h14 M13 6l6 6-6 6',
  menu: 'M4 6h16 M4 12h16 M4 18h16',
  close: 'M6 6l12 12 M18 6 6 18',
  sun: 'M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8 M12 2v2 M12 20v2 M2 12h2 M20 12h2 M5 5l1 1 M18 18l1 1 M5 19l1-1 M18 6l1-1',
  moon: 'M20 15A9 9 0 0 1 9 4a9 9 0 1 0 11 11',
  monitor: 'M3 4h18v13H3z M8 21h8 M12 17v4',
  check: 'M5 12l4 4L19 6',
  alert: 'M12 3 2 21h20z M12 9v5 M12 17v1',
  folder: 'M3 5h7l2 3h9v12H3z',
};

export function Icon({ name, size = 20, className }: { name: IconName; size?: number; className?: string }) {
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className={className}><path d={paths[name]} /></svg>;
}
