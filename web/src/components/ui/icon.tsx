import type { SVGProps } from 'react';
const paths = {
  transactions: 'M4 7h16M4 12h10M4 17h7m6-3 3 3-3 3m3-3h-7',
  disputes: 'M8 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14V5a2 2 0 0 0-2-2h-3M8 2h6v4H8zM7 11h8M7 15h5',
  help: 'M9.1 9a3 3 0 0 1 5.8 1c0 2-3 2-3 4m.1 3h.01M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0',
  arrow: 'M5 12h14m-5-5 5 5-5 5',
  chevron: 'm9 5 7 7-7 7',
  shield: 'm12 3 8 3v6c0 5-8 9-8 9s-8-4-8-9V6l8-3m-4 9 3 3 5-6',
  clock: 'M12 7v5l3 2m7-2a10 10 0 1 1-20 0 10 10 0 0 1 20 0',
  check: 'm5 12 4 4L19 6',
  card: 'M3 8h18M3 12h18M6 16h4M4 4h16a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1',
  alert: 'M12 8v5m0 4h.01M10 3 2 18a2 2 0 0 0 2 3h16a2 2 0 0 0 2-3L14 3a2 2 0 0 0-4 0',
  logout: 'M9 4H4v16h5m5-13 5 5-5 5m-6-5h11',
  user: 'M16 8a4 4 0 1 1-8 0 4 4 0 0 1 8 0M4 21a8 8 0 0 1 16 0',
} as const;
export function Icon({ name, ...props }: SVGProps<SVGSVGElement> & { name: keyof typeof paths }) {
  return (
    <svg
      width="22"
      height="22"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.7"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      {...props}
    >
      <path d={paths[name]} />
    </svg>
  );
}
