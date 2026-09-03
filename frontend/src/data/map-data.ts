export type LayerKey = "players" | "clubs" | "europe";

export type MapItem = {
  id: string;
  layer: LayerKey;
  name: string;
  location: string;
  coordinates: [number, number];
  monogram: string;
  stats: Array<[string, string]>;
  description: string;
  accent: string;
};

export const layerMeta: Array<{
  key: LayerKey;
  label: string;
  count: number;
  color: string;
}> = [
  { key: "players", label: "Player birthplaces", count: 148, color: "#f5eedb" },
  { key: "clubs", label: "Supporters clubs", count: 312, color: "#e5ae52" },
  { key: "europe", label: "European away nights", count: 64, color: "#59c98c" },
];

export const mapItems: MapItem[] = [
  {
    id: "harry-kane",
    layer: "players",
    name: "Harry Kane",
    location: "Walthamstow, London, England",
    coordinates: [-0.0198, 51.588],
    monogram: "HK",
    accent: "#f5eedb",
    stats: [
      ["Spurs appearances", "435"],
      ["Spurs goals", "280"],
      ["Academy joined", "2004"],
      ["First-team debut", "2011"],
    ],
    description:
      "One of 148 charted birthplaces on the map, clustered most densely across North London — pan and zoom to explore the birthplace archive in detail.",
  },
  {
    id: "son-heung-min",
    layer: "players",
    name: "Son Heung-min",
    location: "Chuncheon, South Korea",
    coordinates: [127.7298, 37.8813],
    monogram: "SH",
    accent: "#f5eedb",
    stats: [
      ["Spurs appearances", "454"],
      ["Spurs goals", "173"],
      ["Signed for Spurs", "2015"],
      ["European goals", "29"],
    ],
    description: "A record-setting Lilywhite whose birthplace connects the archive to the Korean peninsula.",
  },
  {
    id: "wellington-spurs",
    layer: "clubs",
    name: "Wellington Spurs",
    location: "Wellington, New Zealand",
    coordinates: [174.7633, -41.2866],
    monogram: "WS",
    accent: "#e5ae52",
    stats: [
      ["Members", "186"],
      ["Founded", "2011"],
      ["Watch parties", "42"],
      ["Distance from N17", "18,852 km"],
    ],
    description: "The furthest club in the current supporters archive, keeping the Lilywhite signal strong from Aotearoa.",
  },
  {
    id: "madrid-away",
    layer: "europe",
    name: "Madrid away night",
    location: "Madrid, Spain",
    coordinates: [-3.7038, 40.4168],
    monogram: "MA",
    accent: "#59c98c",
    stats: [
      ["Competition", "Champions League"],
      ["Season", "2017/18"],
      ["Travelling fans", "3,200"],
      ["Distance from N17", "1,263 km"],
    ],
    description: "A European away night preserved in the archive as one of 64 continental matchday journeys.",
  },
  {
    id: "amsterdam-away",
    layer: "europe",
    name: "Amsterdam away night",
    location: "Amsterdam, Netherlands",
    coordinates: [4.9041, 52.3676],
    monogram: "AA",
    accent: "#59c98c",
    stats: [
      ["Competition", "Champions League"],
      ["Season", "2018/19"],
      ["Travelling fans", "5,000"],
      ["Matchday", "8 May 2019"],
    ],
    description: "A famous European night, mapped alongside the people and places that make the archive live.",
  },
];
